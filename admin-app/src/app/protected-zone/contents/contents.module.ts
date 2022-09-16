import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CatergoriesComponent } from './catergories/catergories.component';
import { KnowledgeBaseComponent } from './knowledge-bases/knowledge-bases.component';
import { CommentsComponent } from './comments/comments.component';
import { ReportsComponent } from './reports/reports.component';
import { ContentsRoutingModule } from './contents-routing.module';



@NgModule({
  declarations: [
    CatergoriesComponent,
    KnowledgeBaseComponent,
    CommentsComponent,
    ReportsComponent
  ],
  imports: [
    CommonModule,
    ContentsRoutingModule
  ]
})
export class ContentsModule { }
