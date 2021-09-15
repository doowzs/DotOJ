import {Component, OnInit } from "@angular/core";
import {Title} from "@angular/platform-browser";
import {FormBuilder} from "@angular/forms";
import {AuthorizeService} from "../authorize.service";
import {ActivatedRoute, Router} from "@angular/router";
import { take } from 'rxjs/operators';
import {faArrowLeft} from '@fortawesome/free-solid-svg-icons';
@Component({
  selector: "auth-login",
  templateUrl: "./profile.component.html",
  styleUrls: ["./profile.component.css"],
})
export class ProfileComponent implements OnInit {
  faArrowLeft = faArrowLeft;
  changeForm = this.formBuilder.group({
    oldPassword: "",
    newPassword: "",
    confirmPassword: ""
  });

  public studentId: string;
  public flag: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private title: Title,
    private authService: AuthorizeService,
    private formBuilder: FormBuilder,
    private authorize: AuthorizeService
  ) {
    title.setTitle("更改密码");
  }
  ngOnInit() {
    this.flag = this.route.snapshot.queryParams.flag;
    this.authorize.getUser()
      .pipe(take(1))
      .subscribe(user => {
        this.studentId = user.username;
          });
  }

  onSubmit(): void {
    if (!!this.studentId) {
      if (this.changeForm.value.newPassword !== this.changeForm.value.confirmPassword) {
        alert("两次密码不一致，请重新输入！");
      }
      else if (this.changeForm.value.newPassword == this.studentId) {
        alert("请不要使用初始密码！");
      }
      else if (this.changeForm.value.newPassword == "") {
        alert("新密码不能为空！");
      }
      else if (this.changeForm.value.newPassword.length < 7) {
        alert("新密码不能低于7位");
      }
      else {
        this.authorize.changePassword(this.changeForm.value.oldPassword, this.changeForm.value.newPassword)
          .subscribe(message => {
            alert(message);
            this.router.navigate(['/']);
          },(err) => {
            alert(err.error);
            }
          );
      }
    }
  }
}
