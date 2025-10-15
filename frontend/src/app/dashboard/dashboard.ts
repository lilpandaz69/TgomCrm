import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css'] // ✅ لاحظ هنا (styleUrls) بصيغة الجمع
})
export class Dashboard implements OnInit {

  username: string | null = null;
  role: string | null = null;

  ngOnInit(): void {
    // 🟢 اقرأ البيانات من localStorage
    this.username = localStorage.getItem('username');
    this.role = localStorage.getItem('role');
  }

}
