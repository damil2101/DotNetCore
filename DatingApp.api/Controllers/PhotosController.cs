using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.api.Data;
using DatingApp.api.Dtos;
using DatingApp.api.Helpers;
using DatingApp.api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.api.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;
        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

            //setting up cloudinary account, this matches to the cloudinary app settings
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }
        [HttpGet("{id}",Name="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            Photo photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,[FromForm]PhotoForCreationDto photoDto)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            User userFromRepo = await _repo.GetUser(userId);
             
             var file = photoDto.File;
             if(file == null)
                return BadRequest("No File found");
             var uploadResult = new ImageUploadResult();
             if(file.Length > 0)
             {
                 using(var stream = file.OpenReadStream())
                 {
                     var uploadParams = new ImageUploadParams()
                     {
                         File = new FileDescription(file.Name, stream),
                         Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                     };

                     uploadResult = _cloudinary.Upload(uploadParams);

                 }
             } 
            // photo uploaded to cloudinary
             photoDto.Url = uploadResult.Uri.ToString();
             photoDto.PublicId = uploadResult.PublicId;

            //map photo dto to Model
             Photo photo = _mapper.Map<Photo>(photoDto);

             if(!userFromRepo.Photos.Any(u=>u.IsMain))
                photo.IsMain = true;

             userFromRepo.Photos.Add(photo);
     

             if(await _repo.SaveAll())
             {
                 var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                 return CreatedAtRoute("GetPhoto",new {userId = userFromRepo.Id,id = photo.Id}, photoToReturn);
             }

             return BadRequest("Could not add photo");   
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            User user = await _repo.GetUser(userId);
            if(user == null)
                return Unauthorized();

            if(!user.Photos.Any(p=>p.Id == id))
                return Unauthorized();

            var photofromRepo = await _repo.GetPhoto(id);

            if(photofromRepo.IsMain)
                return BadRequest("This is already main photo");

            var currentMainPhoto = await _repo.GetMainPhoto(userId);
            currentMainPhoto.IsMain = false;
            photofromRepo.IsMain = true;
            if(await _repo.SaveAll())
                return NoContent();
            else
                return BadRequest("Could not set photo to main");            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId ,int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            User user = await _repo.GetUser(userId);
            if(user == null)
                return Unauthorized();

            if(!user.Photos.Any(p=>p.Id == id))
                return Unauthorized();

            var photofromRepo = await _repo.GetPhoto(id);

            //not allow them to delete main photo
            if(photofromRepo.IsMain)
                return BadRequest("You cannot delete main photo");

            // if a photo is uploaded to cloudinary    
            if(photofromRepo.PublicId != null)
            {
                    var deleteParams = new DeletionParams(photofromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok") //cloudinary delete result for successful deletion
                {
                    _repo.Delete(photofromRepo);
                }
            }
            if(photofromRepo.PublicId == null)
            {
                _repo.Delete(photofromRepo);
            }
           
            if(await _repo.SaveAll())
                return  Ok();
            else 
                return BadRequest("Failed to delete the photo");        

        }
    }
}