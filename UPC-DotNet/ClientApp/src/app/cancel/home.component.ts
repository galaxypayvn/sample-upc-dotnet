import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  constructor(private route: ActivatedRoute, private router: Router) { }

  param1: string;
  param2: string;

  ngOnInit() {
    console.log('Called Constructor');
    this.route.queryParams.subscribe(params => {
      this.param1 = JSON.parse(params['param1']);
      this.param2 = JSON.parse(params['param2']);
    });
  }
}
