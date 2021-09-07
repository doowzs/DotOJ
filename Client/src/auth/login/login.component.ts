import { Component } from "@angular/core";
import { Title } from "@angular/platform-browser";
import { FormBuilder } from "@angular/forms";
import { AuthorizeService } from "../authorize.service";
import { ActivatedRoute, Route, Router } from "@angular/router";

@Component({
  selector: "auth-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.css"],
})
export class LoginComponent {
  loginForm = this.formBuilder.group({
    username: "",
    password: "",
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private title: Title,
    private authService: AuthorizeService,
    private formBuilder: FormBuilder
  ) {
    title.setTitle("请登录OJ");
  }

  onSubmit(): void {
    this.authService
      .authenticate(
        this.loginForm.value.username,
        this.loginForm.value.password
      )
      .subscribe(
        () => {
          this.router.navigateByUrl(
            this.route.snapshot.queryParams.redirect ?? "/",
            {
              replaceUrl: true,
            }
          );
        },
        (error) => {
          alert(error.error);
        }
      );
  }
  tips(): void {
    alert("请联系《问题求解》课程助教191220186。");
  }
}
