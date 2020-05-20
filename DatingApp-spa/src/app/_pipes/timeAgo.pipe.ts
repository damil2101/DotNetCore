import { Pipe } from "@angular/core";
import { TimeAgoPipe } from 'time-ago-pipe';

@Pipe({
    name: 'timeAgo',
    pure: false
})
export class TimeAgoExtendsPipe extends TimeAgoPipe {}

//TimeAGo pipe module is not supported on Angular 9. follow the link 
//https://github.com/AndrewPoyntz/time-ago-pipe/issues/33
