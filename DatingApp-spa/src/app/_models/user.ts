import { Photo } from './Photo';

export interface User {
    id: number;
    username:string;
    knownAs:string;
    age:number;
    gender:string;
    created:Date;
    lastActive:Date;
    photoUrl:string;
    city:string;
    country:string;
    myProperty:number;
    
    //optional properties always defined later
    interests?:string;
    introduction?:string;
    lookingFor?:string;
    photos?:Photo[]
}
