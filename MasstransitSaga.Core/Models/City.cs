using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasstransitSaga.Core.Models;

[Table("city")]
public partial class City
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public string CountryCode { get; set; }

    public string District { get; set; }

    public int Population { get; set; }

    public virtual Country CountryCodeNavigation { get; set; }
}
