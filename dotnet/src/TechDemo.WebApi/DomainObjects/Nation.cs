﻿using System.ComponentModel.DataAnnotations;
using TechDemo.WebApi.Entities;

namespace TechDemo.WebApi.DomainObjects {
  [PrimaryIdentity(nameof(Id))]
  [UniquePropertyGroup(nameof(Id), nameof(Id))]
  [PluralName("Nations")]
  public class Nation {
    public int Id { get; set; }
    [IdentityLabel]
    public string Name { get; set; } = string.Empty;
    public NationCode Code { get; set; }
  }
}
