import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CatergoriesComponent } from './catergories/catergories.component';
import { CommentsComponent } from './comments/comments.component';
import { KnowledgeBaseComponent } from './knowledge-bases/knowledge-bases.component';
import { ReportsComponent } from './reports/reports.component';
const routes: Routes = [
    {
        path: '',
        component: KnowledgeBaseComponent
    },
    {
        path: 'KnowledgeBase',
        component: KnowledgeBaseComponent
    },
    {
        path: 'Categories',
        component: CatergoriesComponent
    },
    {
        path: 'Comments',
        component: CommentsComponent
    },
    {
        path: 'Reports',
        component: ReportsComponent
    },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ContentsRoutingModule {}
