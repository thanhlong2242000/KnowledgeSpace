using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.ViewModels.Systems;
using KnowledgeSpace.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using KnowledgeSpace.BackendServer.Helpers;
using KnowledgeSpace.BackendServer.Autherization;
using KnowledgeSpace.BackendServer.Constants;
using Serilog;
using Microsoft.Extensions.Logging;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class FunctionsController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FunctionsController> _logger;
        public FunctionsController(ApplicationDbContext context, ILogger<FunctionsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        [ApiValidationFilter]
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            _logger.LogInformation("Begin PostFunction API");
            var dbFunction = await _context.Functions.FindAsync(request.Id);
            if (dbFunction != null)
                return BadRequest(new ApiBadRequestResponse($"Function with id {request.Id} is existed."));
            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                Url = request.Url,
            };
            _context.Functions.Add(function);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation("End PostFunction API - Success");
                return CreatedAtAction(nameof(GetById), new { id = function.Id }, request);
            }
            else
            {
                _logger.LogInformation("End PostFunction API - Failed");
                return BadRequest(new ApiBadRequestResponse("create function is fail"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFunctions()
        {
            var functions = _context.Functions;

            var functionvms = await functions.Select(u => new FunctionVm()
            {
                Id = u.Id,
                Name = u.Name,
                ParentId = u.ParentId,
                SortOrder = u.SortOrder,
                Url = u.Url,
            }).ToListAsync();

            return Ok(functionvms);
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetFunctionsPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Functions.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Id.Contains(filter)
                || x.Name.Contains(filter)
                || x.Id.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new FunctionVm()
                {
                    Id = u.Id,
                    Name = u.Name,
                    ParentId = u.ParentId,
                    SortOrder = u.SortOrder,
                    Url = u.Url,
                })
                .ToListAsync();

            var pagination = new Pagination<FunctionVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound(new ApiNotFoundRespone($"Cannot found function with Id{id}"));

            var functionVm = new FunctionVm()
            {
                Id = function.Id,
                Name = function.Name,
                ParentId = function.ParentId,
                SortOrder = function.SortOrder,
                Url = function.Url,
            };
            return Ok(functionVm);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutFunction(string id, [FromBody] FunctionCreateRequest request)
        {

            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound(new ApiNotFoundRespone($"Cannot found function with Id{id}"));

            function.Name = request.Name;
            function.ParentId = request.ParentId;
            function.SortOrder = request.SortOrder;
            function.Url = request.Url;

            _context.Functions.Update(function);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound(new ApiNotFoundRespone($"Cannot found function with Id{id}"));

            _context.Functions.Remove(function);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var functionvm = new FunctionVm()
                {
                    Id = function.Id,
                    Name = function.Name,
                    ParentId = function.ParentId,
                    SortOrder = function.SortOrder,
                    Url = function.Url,
                };
                return Ok(functionvm);
            }
            return BadRequest();
        }

        [HttpGet("{functionId}/commands")]
        public async Task<IActionResult> GetCommandInFunction(string functionId)
        {
            var query = from a in _context.Commands
                        join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
                        from commandInFunction in result1.DefaultIfEmpty()
                        join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
                        from function in result2.DefaultIfEmpty()
                        select new
                        {
                            a.Id,
                            a.Name,
                            commandInFunction.FunctionId
                        };

            query = query.Where(x => x.FunctionId == functionId);

            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
            return Ok(data);
        }

        [HttpPost("{functionId}/commands/not-in-function")]
        public async Task<IActionResult> GetCommandsNotFunction(string functionId)
        {
            var query = from a in _context.Commands
                        join cif in _context.CommandInFunctions on a.Id equals cif.CommandId into result1
                        from commandInFunction in result1.DefaultIfEmpty()
                        join f in _context.Functions on commandInFunction.FunctionId equals f.Id into result2
                        from function in result2.DefaultIfEmpty()
                        select new
                        {
                            a.Id,
                            a.Name,
                            commandInFunction.FunctionId
                        };
            query = query.Where(x => x.FunctionId != functionId).Distinct();

            var data = await query.Select(x => new CommandVm()
            {
                Id = x.Id,
                Name = x.Name,
            }).ToListAsync();
            return Ok(data);
        }

        [HttpPost("{functionId}/commands")]
        public async Task<IActionResult> PostCommandToFunction(string functionId, [FromBody] AddCommandToFunctionRequest request)
        {
            var commandFunction = await _context.CommandInFunctions.FindAsync(request.CommandId, request.FunctionId);
            if(commandFunction != null)
                return BadRequest(new ApiBadRequestResponse($"this command has been added to function"));
            var entity = new CommandInFunction()
            {
                CommandId = request.CommandId,
                FunctionId = request.FunctionId
            };
            _context.CommandInFunctions.Add(entity);
            var result = await _context.SaveChangesAsync();

            if(result > 0)
            {
                return CreatedAtAction(nameof(GetById), new
                {
                    CommandId = request.CommandId,
                    FunctionId = request.FunctionId
                }, request);
            }
            else{
                return BadRequest(new ApiBadRequestResponse("Add command to function failed"));
            }
        }

        [HttpDelete("{functionId}/commands/{commandId}")]
        public async Task<IActionResult> DeleteCommandToFunction(string functionId, string commandId)
        {
            var commandFunction = await _context.CommandInFunctions.FindAsync(commandId, functionId);
            if (commandFunction != null)
                return BadRequest(new ApiBadRequestResponse($"this command has been added to function"));
            var entity = new CommandInFunction()
            {
                CommandId = commandId,
                FunctionId = functionId
            };
            _context.CommandInFunctions.Remove(entity);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Delete command to function failed");
            }
        }
    }
}
