﻿using CatologServiceAPI.Core.Domain;
using CatologServiceAPI.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CatologServiceAPI.Infrastructure.Entityconfigurations
{
    class CatalogTypeEntityTypeConfiguration: IEntityTypeConfiguration<CatalogType>
    {
        public void Configure(EntityTypeBuilder<CatalogType> builder)
        {
            builder.ToTable("CatalogType", CatalogContext.DEFAULT_SCHEMA);

            builder.HasKey(ci => ci.Id);

            builder.Property(ci => ci.Id)
               .UseHiLo("catalog_type_hilo")
               .IsRequired();

            builder.Property(cb => cb.Type)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
