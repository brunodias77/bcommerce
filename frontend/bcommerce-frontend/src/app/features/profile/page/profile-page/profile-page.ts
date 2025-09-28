import { Component } from '@angular/core';
import { Container } from "../../../../shared/components/container/container";
import { Button } from "../../../../shared/components/button/button";

@Component({
  selector: 'app-profile-page',
  imports: [Container, Button],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.css'
})
export class ProfilePage {

}
