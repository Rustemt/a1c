//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace A1fxCrm.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class CustomerTicket
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public int CustomerId { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd:MM:yyyy HH:mm:ss}", ApplyFormatInEditMode = true)]
        public System.DateTime CreatedDate { get; set; }
    
        public virtual Customer Customer { get; set; }
    }
}
