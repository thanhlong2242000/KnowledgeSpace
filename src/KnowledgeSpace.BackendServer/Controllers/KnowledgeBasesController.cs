using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.ViewModels.Systems;
using KnowledgeSpace.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using KnowledgeSpace.ViewModels.Contents;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using IdentityServer4.Models;
using System.Xml.Linq;
using KnowledgeSpace.BackendServer.Services;
using KnowledgeSpace.BackendServer.Helpers;
using System.Net.Http.Headers;
using System.IO;
using KnowledgeSpace.BackendServer.Autherization;
using KnowledgeSpace.BackendServer.Constants;
using Microsoft.Extensions.Logging;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class KnowledgeBasesController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly ISequenceService _sequenceService;
        private readonly IStorageService _storageService;
        private readonly ILogger<KnowledgeBasesController> _logger;
        public KnowledgeBasesController(ApplicationDbContext context, ISequenceService sequenceService, IStorageService storageService, ILogger<KnowledgeBasesController> logger)
        {
            _context = context;
            _sequenceService = sequenceService;
            _storageService = storageService;
            _logger = logger;
        }

        #region KnowledgeBase

        [HttpPost]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.CREATE)]
        public async Task<IActionResult> PostKnowledgeBase([FromForm] KnowledgeBaseCreateRequest request)
        {
            _logger.LogInformation("Begin PostKnowledgeBase API");
            var knowledgeBase = new KnowledgeBase()
            {
                CategoryId = request.CategoryId,

                Title = request.Title,

                SeoAlias = request.SeoAlias,

                Description = request.Description,

                Environment = request.Environment,

                Problem = request.Problem,

                StepToReproduce = request.Problem,

                ErrorMessage = request.ErrorMessage,

                Workaround = request.Workaround,

                Note = request.Note,

                Labels = request.Labels
            };
            knowledgeBase.Id = await _sequenceService.GetKnowledgeBaseNewId();

            _context.KnowledgeBases.Add(knowledgeBase);

            //Process Attachment
            if(request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    var attachmentEntity = await SaveFile(knowledgeBase.Id, attachment);
                    _context.Attachments.Add(attachmentEntity);
                }
            }
            _context.KnowledgeBases.Add(knowledgeBase);

            //Process Label
            if (string.IsNullOrEmpty(request.Labels))
            {
                await ProcessLabel(request, knowledgeBase);
            }

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                _logger.LogInformation("End PostKnowledgeBase API - Success");
                return CreatedAtAction(nameof(GetById), new 
                { 
                    id = knowledgeBase.Id 
                }, request);
            }
            else
            {
                _logger.LogInformation("End PostKnowledgeBase API - Failed");
                return BadRequest(new ApiBadRequestResponse("Create knowledge failed"));
            }
        }
        private async Task<Attachment>SaveFile(int knowledegeBaseId, IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            var attachmentEntity = new Attachment
            {
                FileName = fileName,
                FilePath = _storageService.GetFileUrl(fileName),
                FileSize = file.Length,
                FileType = Path.GetExtension(fileName),
                KnowledgeBaseId = knowledegeBaseId,
            };
            return attachmentEntity;
        }

        private async Task ProcessLabel(KnowledgeBaseCreateRequest request, KnowledgeBase knowledgeBase)
        {
            string[] labels = request.Labels.Split(',');
            foreach (var labelText in labels)
            {
                var labelId = TextHelper.ToUnsignString(labelText);
                var existingLabel = await _context.Labels.FindAsync(labelId);
                if (existingLabel == null)
                {
                    var labelEntity = new Label()
                    {
                        Id = labelId,
                        Name = labelText,
                    };
                    _context.Labels.Add(labelEntity);
                }
                var labelInKnowledgeBase = new LabelInKnowledgeBase()
                {
                    KnowledgeBaseId = knowledgeBase.Id,
                    LabelId = labelId,
                };
                _context.LabelInKnowledgeBases.Add(labelInKnowledgeBase);
            }
        }

        [HttpGet]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.VIEW)]
        public async Task<IActionResult> GetKnowledgeBases()
        {
            var knowledgeBases = _context.KnowledgeBases;

            var knowledgeBasevms = await knowledgeBases.Select(u => new KnowledgeBaseQuickVm()
            {
                Id = u.Id,
                CategoryId = u.CategoryId,
                Description = u.Description,
                SeoAlias = u.SeoAlias,
                Title = u.Title,
            }).ToListAsync();

            return Ok(knowledgeBasevms);
        }

        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.VIEW)]
        public async Task<IActionResult> GetKnowledgeBasesPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.KnowledgeBases.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Title.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new KnowledgeBaseQuickVm()
                {
                    Id = u.Id,
                    CategoryId = u.CategoryId,
                    Description = u.Description,
                    SeoAlias = u.SeoAlias,
                    Title = u.Title,
                }).ToListAsync();

            var pagination = new Pagination<KnowledgeBaseQuickVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(int id)
        {
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
            if (knowledgeBase == null)
                return NotFound();

            var knowledgeBaseVm  = CreateKnowledgeBaseVm(knowledgeBase);

            return Ok(knowledgeBaseVm);
        }

        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.UPDATE)]
        public async Task<IActionResult> PutKnowledgeBase(int id, [FromBody] KnowledgeBaseCreateRequest request)
        {

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
            if (knowledgeBase == null)
                return NotFound();

                knowledgeBase.Id = knowledgeBase.Id;

                knowledgeBase.CategoryId = knowledgeBase.CategoryId;

                knowledgeBase.Title = knowledgeBase.Title;

                knowledgeBase.SeoAlias = knowledgeBase.SeoAlias;

                knowledgeBase.Description = knowledgeBase.Description;

                knowledgeBase.Environment = knowledgeBase.Environment;

                knowledgeBase.Problem = knowledgeBase.Problem;

                knowledgeBase.StepToReproduce = knowledgeBase.StepToReproduce;

                knowledgeBase.ErrorMessage = knowledgeBase.ErrorMessage;

                knowledgeBase.Workaround = knowledgeBase.Workaround;

                knowledgeBase.Note = knowledgeBase.Note;

                knowledgeBase.OwnerUserId = knowledgeBase.OwnerUserId;

                knowledgeBase.Labels = knowledgeBase.Labels;

                knowledgeBase.CreateDate = knowledgeBase.CreateDate;

                knowledgeBase.LastModifiedDate = knowledgeBase.LastModifiedDate;

                knowledgeBase.NumberOfComments = knowledgeBase.NumberOfComments;

                knowledgeBase.NumberOfVotes = knowledgeBase.NumberOfVotes;

                knowledgeBase.NumberOfReports = knowledgeBase.NumberOfReports;

            _context.KnowledgeBases.Update(knowledgeBase);

            if (!string.IsNullOrEmpty(request.Labels))
            {
                await ProcessLabel(request, knowledgeBase);
            }

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.CONTENT_KNOWLEDGEBASE, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteKnowledgeBase(string id)
        {
            var knowledgeBase = await _context.KnowledgeBases.FindAsync(id);
            if (knowledgeBase == null)
                return NotFound();

            _context.KnowledgeBases.Remove(knowledgeBase);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                KnowledgeBaseVm knowledgeBasevm = CreateKnowledgeBaseVm(knowledgeBase);
                return Ok(knowledgeBasevm);
            }
            return BadRequest();
        }

        private static KnowledgeBaseVm CreateKnowledgeBaseVm(KnowledgeBase knowledgeBase)
        {
            return new KnowledgeBaseVm()
            {
                Id = knowledgeBase.Id,

                CategoryId = knowledgeBase.CategoryId,

                Title = knowledgeBase.Title,

                SeoAlias = knowledgeBase.SeoAlias,

                Description = knowledgeBase.Description,

                Environment = knowledgeBase.Environment,

                Problem = knowledgeBase.Problem,

                StepToReproduce = knowledgeBase.StepToReproduce,

                ErrorMessage = knowledgeBase.ErrorMessage,

                Workaround = knowledgeBase.Workaround,

                Note = knowledgeBase.Note,

                OwnerUserId = knowledgeBase.OwnerUserId,

                Labels = knowledgeBase.Labels,

                CreateDate = knowledgeBase.CreateDate,

                LastModifiedDate = knowledgeBase.LastModifiedDate,

                NumberOfComments = knowledgeBase.NumberOfComments,

                NumberOfVotes = knowledgeBase.NumberOfVotes,

                NumberOfReports = knowledgeBase.NumberOfReports,
            };
        }

        #endregion KnowledgeBase


        #region Comments

        [HttpGet("{KnowledgeBaseId}/comments/filter")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommentsPaging(int KnowledgeBaseId, string filter, int pageIndex, int pageSize)
        {
            var query = _context.Comments.Where(x => x.KnowledgeBaseId == KnowledgeBaseId).AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Content.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(c => new CommentVm()
                {
                    Id = c.Id,
                    Content = c.Content,
                    KnowledgeBaseId = c.KnowledgeBaseId,
                    OwnwerUserId = c.OwnwerUserId,
                    CreateDate = c.CreateDate,
                    LastModifiedDate= c.LastModifiedDate,
                }).ToListAsync();

            var pagination = new Pagination<CommentVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{KnowledgeBaseId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.VIEW)]
        public async Task<IActionResult> GetCommentDetail(string commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound();

            var commentVm = new CommentVm()
            {
                Id = comment.Id,
                Content = comment.Content,
                KnowledgeBaseId = comment.KnowledgeBaseId,
                OwnwerUserId = comment.OwnwerUserId,
                CreateDate = comment.CreateDate,
                LastModifiedDate = comment.LastModifiedDate,
            };

            return Ok(commentVm);
        }

        [HttpPost("{KnowledgeBaseId}/comments")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.CREATE)]
        public async Task<IActionResult> PostCommentDetail(int KnowledgeBaseId, [FromBody] CommentCreateRequest request)
        {
            var comment = new Comment()
            {
                Content = request.Content,
                KnowledgeBaseId = request.KnowledgeBaseId,
                OwnwerUserId = String.Empty, /*TODO: GET USER FROM CLAIM*/
            };
            _context.Comments.Add(comment);

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(KnowledgeBaseId);
            if (knowledgeBase != null)
                return BadRequest();
            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfComments.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = KnowledgeBaseId, commentId = comment.Id }, request);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{KnowledgeBaseId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.UPDATE)]
        public async Task<IActionResult> PutComment(int commentId , [FromBody] CommentCreateRequest request)
        {

            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound();
            if (comment.OwnwerUserId != User.Identity.Name)
                return Forbid();

            comment.Content = request.Content;
            _context.Comments.Update(comment);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpDelete("{KnowledgeBaseId}/comments/{commentId}")]
        [ClaimRequirement(FunctionCode.CONTENT_COMMENT, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteComment(int commentId, int KnowledgeBaseId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return NotFound();

            _context.Comments.Remove(comment);

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(KnowledgeBaseId);
            if (knowledgeBase != null)
                return BadRequest();
            knowledgeBase.NumberOfComments = knowledgeBase.NumberOfComments.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var commentVm = new CommentVm()
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    KnowledgeBaseId = comment.KnowledgeBaseId,
                    OwnwerUserId = comment.OwnwerUserId,
                    CreateDate = comment.CreateDate,
                    LastModifiedDate = comment.LastModifiedDate,
                };
                return Ok(commentVm);
            }
            return BadRequest();
        }

        #endregion Comments


        #region Votes
        [HttpGet("{KnowledgeBaseId}/votes")]
        public async Task<IActionResult> GetVotes(int KnowledgeBaseId)
        {
            var votes = await _context.Votes.Where(x => x.KnowledgeBaseId == KnowledgeBaseId)
                .Select(x => new VoteVm()
            {
                UserId = x.UserId,
                KnowledgeBaseId = x.KnowledgeBaseId,
                CreateDate = x.CreateDate,
                LastModifiedDate = x.LastModifiedDate,
            }).ToListAsync();

            return Ok(votes);
        }

        [HttpPost("{KnowledgeBaseId}/votes")]
        public async Task<IActionResult> PostVotes(int KnowledgeBaseId, [FromBody] VoteCreateRequest request)
        {
            var vote = await _context.Votes.FindAsync(KnowledgeBaseId , request.UserId);
            if(vote != null)
                return BadRequest("This user has been voted for this KB");
            vote = new Vote()
            {
                KnowledgeBaseId = KnowledgeBaseId,
                UserId = request.UserId,
            };
            _context.Votes.Add(vote);

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(KnowledgeBaseId);
            if (knowledgeBase != null)
                return BadRequest();
            knowledgeBase.NumberOfVotes = knowledgeBase.NumberOfVotes.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("{KnowledgeBaseId}/votes/{userId}")]
        public async Task<IActionResult> DeleteVotes(int KnowledgeBaseId, string userId)
        {
            var vote = await _context.Votes.FindAsync(KnowledgeBaseId, userId);
            if (vote == null)
                return NotFound();

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(KnowledgeBaseId);
            if (knowledgeBase != null)
                return BadRequest();
            knowledgeBase.NumberOfVotes = knowledgeBase.NumberOfVotes.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);

            _context.Votes.Remove(vote);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }
        #endregion Votes


        #region Report

        [HttpGet("{KnowledgeBaseId}/reports/filter")]
        public async Task<IActionResult> GetReportsPaging(int KnowledgeBaseId, string filter, int pageIndex, int pageSize)
        {
            var query = _context.Reports.Where(x => x.KnowledgeBaseId == KnowledgeBaseId).AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Content.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(c => new ReportVm()
                {
                    Id = c.Id,
                    Content = c.Content,
                    KnowledgeBaseId = c.KnowledgeBaseId,
                    IsProcessed = false,
                    ReportUserId = c.ReportUserId,
                    CreateDate = c.CreateDate,
                    LastModifiedDate = c.LastModifiedDate,
                }).ToListAsync();

            var pagination = new Pagination<ReportVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{KnowledgeBaseId}/reports/{reportId}")]
        public async Task<IActionResult> GetReportsDetail(int KnowledgeBaseId, int reporId)
        {
            var report = await _context.Reports.FindAsync(reporId);
            if (report == null)
                return NotFound();

            var reportVm = new ReportVm()
            {
                Id = report.Id,
                Content = report.Content,
                KnowledgeBaseId = report.KnowledgeBaseId,
                IsProcessed=false,
                ReportUserId= report.ReportUserId,
                CreateDate = report.CreateDate,
                LastModifiedDate = report.LastModifiedDate,
            };

            return Ok(reportVm);
        }

        [HttpPost("{KnowledgeBaseId}/reports")]
        public async Task<IActionResult> PostReport(int KnowledgeBaseId, [FromBody] ReportCreateRequest request)
        {
            var report = new Report()
            {
                Content = request.Content,
                KnowledgeBaseId = request.KnowledgeBaseId,
                ReportUserId = request.ReportUserId,
                IsProcessed = false,
            };
            _context.Reports.Add(report);

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(KnowledgeBaseId);
            if (knowledgeBase != null)
                return BadRequest();
            knowledgeBase.NumberOfReports = knowledgeBase.NumberOfReports.GetValueOrDefault(0) + 1;
            _context.KnowledgeBases.Update(knowledgeBase);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{KnowledgeBaseId}/reports/{reportId}")]
        public async Task<IActionResult> PutReport(int reportId, [FromBody] CommentCreateRequest request)
        {

            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound();
            if (report.ReportUserId != User.Identity.Name)
                return Forbid();

            report.Content = request.Content;
            _context.Reports.Update(report);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpDelete("{KnowledgeBaseId}/reports/{reportId}")]
        public async Task<IActionResult> DeleteReport(int reportId, int KnowledgeBaseId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
                return NotFound();

            _context.Reports.Remove(report);

            var knowledgeBase = await _context.KnowledgeBases.FindAsync(KnowledgeBaseId);
            if (knowledgeBase != null)
                return BadRequest();
            knowledgeBase.NumberOfReports = knowledgeBase.NumberOfReports.GetValueOrDefault(0) - 1;
            _context.KnowledgeBases.Update(knowledgeBase);

            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        #endregion Report


        #region Attachment

        [HttpGet("{KnowledgeBaseId}/attachments")]
        public async Task<IActionResult> GetAttachment(int KnowledgeBaseId)
        {
            var query = await _context.Attachments
                .Where(x => x.KnowledgeBaseId == KnowledgeBaseId)
                .Select(c => new AttachmentVm()
                {
                    Id = c.Id,
                    FileName = c.FileName,
                    FilePath = c.FilePath,
                    FileSize = c.FileSize,
                    FileType = c.FileType,
                    KnowledgeBaseId = c.KnowledgeBaseId,
                    CreateDate = c.CreateDate,
                    LastModifiedDate = c.LastModifiedDate,
                }).ToListAsync();
            return Ok(query);
        }

        [HttpDelete("{KnowledgeBaseId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteAttachment(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment == null)
                return NotFound();

            _context.Attachments.Remove(attachment);


            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok();
            }
            return BadRequest();
        }

        #endregion Attachment
    }
}
