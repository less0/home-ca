import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AuthModule } from '@auth0/auth0-angular';
import { MatButtonModule } from '@angular/material/button'
import { MatToolbarModule } from '@angular/material/toolbar'
import { MatIconModule } from '@angular/material/icon'
import { MatSidenavModule } from '@angular/material/sidenav'
import { MatExpansionModule } from '@angular/material/expansion';
import { MatCardModule } from '@angular/material/card'
import { NavigationComponent } from './navigation/navigation.component';
import { MainComponent } from './main/main.component';
import { HomeComponent } from './home/home.component'
import { EnvServiceFactory, EnvServiceProvider } from './env.service.provider';
import { EnvService } from './env.service';

const env = EnvServiceFactory() as EnvService;
console.log(env);

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    MainComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    AuthModule.forRoot({
      domain: env.AUTH0_DOMAIN,
      clientId: env.AUTH0_CLIENT_ID,
      authorizationParams: {
        redirect_uri: window.location.origin
      }
    }),

    AppRoutingModule,
    BrowserAnimationsModule,
    MatButtonModule,
    MatToolbarModule,
    MatIconModule,
    MatSidenavModule,
    MatExpansionModule,
    MatCardModule
  ],
  providers: [EnvServiceProvider],
  bootstrap: [AppComponent]
})
export class AppModule { }
