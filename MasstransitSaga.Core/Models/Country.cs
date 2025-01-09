using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MasstransitSaga.Core.Models;

[Table("country")]
public partial class Country
{
    [Key]
    public string Code { get; set; }

    public string Name { get; set; }

    public string Continent { get; set; }

    public string Region { get; set; }

    public decimal SurfaceArea { get; set; }

    public short? IndepYear { get; set; }

    public int Population { get; set; }

    public decimal? LifeExpectancy { get; set; }

    public decimal? Gnp { get; set; }

    public decimal? Gnpold { get; set; }

    public string LocalName { get; set; }

    public string GovernmentForm { get; set; }

    public string HeadOfState { get; set; }

    public int? Capital { get; set; }

    public string Code2 { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();

    public virtual ICollection<Countrylanguage> Countrylanguages { get; set; } = new List<Countrylanguage>();
}
