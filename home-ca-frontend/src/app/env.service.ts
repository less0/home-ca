import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class EnvService {
  public AUTH0_DOMAIN = '';
  public AUTH0_CLIENT_ID = '';
}
