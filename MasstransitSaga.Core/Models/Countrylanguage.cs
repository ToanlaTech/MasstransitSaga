using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasstransitSaga.Core.Models;

[Table("countrylanguage")]
public partial class Countrylanguage
{

    public string CountryCode { get; set; }
    [Key]
    public string Language { get; set; }

    public string IsOfficial { get; set; }

    public decimal Percentage { get; set; }

    public virtual Country CountryCodeNavigation { get; set; }
}
