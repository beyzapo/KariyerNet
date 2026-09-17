using System;
using System.Collections.Generic;
using System.Text;

namespace KariyerNet.Domain.Entities
{
    public class CandidateProfile //every user must havent a candidate profile, this entity is linked to the User entity via a foreign key relationship
    {
        public Guid Id { get; set; } ///unique identifier for the candidate profile
        
        public Guid UserId { get; set; } //foreign key to the User entity
        public User User { get; set; } = default; //navigation property to the User entity

       public string CvFilePath { get; set; } = default; //file path to the candidate's CV
       public DateTime CvUploadedAt { get; set; }  = DateTime.UtcNow; //timestamp for when the CV was uploaded

    }


}
