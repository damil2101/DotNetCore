import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { Photo } from 'src/app/_models/Photo';
import {environment} from '../../../environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { error } from '@angular/compiler/src/util';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
@Input() photos: Photo[];
@Output() getMemberPhotoChange = new EventEmitter<string>();
  currentMainPhoto:Photo;
  uploader:FileUploader;
  hasBaseDropZoneOver = false;
  response:string;
  baseUrl = environment.apiUrl;

  constructor(private authService:AuthService,private userService:UserService,private alertify:AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e:any):void {
    this.hasBaseDropZoneOver = e;
  }

  setMainPhoto(photo:Photo){
    this.userService.setMainPhoto(this.authService.decodedToken.nameid,photo.id)
    .subscribe(()=> {
      this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
      this.currentMainPhoto.isMain = false;
      photo.isMain = true;
      this.getMemberPhotoChange.emit(photo.url);
      this.authService.changeMemberPhoto(photo.url);
      this.authService.currentUser.photoUrl = photo.url;
      //modify user item in local storage after changing photo to update the photo url on current user
      localStorage.setItem('user',JSON.stringify(this.authService.currentUser));
    },error => {
      this.alertify.error(error);
    });
  }

  deletePhoto(photoId:number){
    this.alertify.confirm("Are you sure, you want to delete this photo", () => {
      this.userService.deletePhoto(this.authService.decodedToken.nameid,photoId)
      .subscribe(() => {
        this.photos.splice(this.photos.findIndex(p => p.id == photoId),1);
        this.alertify.success("Photo Deleted");
      },(err) => {
        this.alertify.error("Failed to delete photo");
      });
    })
  }

  initializeUploader(){
    this.uploader = new FileUploader({
      url:this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer '+localStorage.getItem('token'),
      allowedFileType:['image'],
      removeAfterUpload:true,
      autoUpload:false,
      maxFileSize:10*1024*1024 // max 10 mb
    });

    this.uploader.onAfterAddingFile = (file) => {file.withCredentials = false;};

    this.uploader.onSuccessItem = (item,response,status,headers) => {
      if(response){
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url:res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);

        if(photo.isMain){
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          //modify user item in local storage after changing photo to update the photo url on current user
          localStorage.setItem('user',JSON.stringify(this.authService.currentUser));
        }
      }
    }
  }

}
