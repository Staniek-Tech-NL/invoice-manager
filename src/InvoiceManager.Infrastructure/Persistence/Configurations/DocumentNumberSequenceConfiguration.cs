using InvoiceManager.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceManager.Infrastructure.Persistence.Configurations;

internal sealed class DocumentNumberSequenceConfiguration : IEntityTypeConfiguration<DocumentNumberSequence>
{
    public void Configure(EntityTypeBuilder<DocumentNumberSequence> builder)
    {
        builder.ToTable("DocumentNumberSequences");
        builder.HasKey(sequence => sequence.Id);

        builder.Property(sequence => sequence.DocumentType).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(sequence => new { sequence.DocumentType, sequence.Year }).IsUnique();
    }
}
