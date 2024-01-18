import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainComponent } from './main/main.component';
import { authGuard } from './auth.guard';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  {
    path: "main",
    component: MainComponent,
    canActivate: [authGuard]
  },
  {
    path: "",
    component: HomeComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
