import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './shared';

const routes: Routes = [
    {
        path: '',
        loadChildren: () => import('./protected-zone/protected-zone.module').then((m) => m.ProtectedZoneModule),
        canActivate: [AuthGuard]
    },
    {
        path: 'login',
        loadChildren: () => import('./login/login.module').then((m) => m.LoginModule),
    },
    {
        path: 'error',
        loadChildren: () => import('./server-error/server-error.module').then((m) => m.ServerErrorModule)
    },
    {
        path: 'access-denied',
        loadChildren: () => import('./access-denied/access-denied.module').then((m) => m.AccessDeniedModule)
    },
    { path: 'not-found', loadChildren: () => import('./not-found/not-found.module').then((m) => m.NotFoundModule) },
    { path: '**', redirectTo: 'not-found' }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' })],
    exports: [RouterModule]
})
export class AppRoutingModule {}
