using System.Text.Json.Nodes;
using FluentAssertions;
using PropertyKwikCheck.Core.Domain;
using PropertyKwikCheck.Core.Mapping;

namespace PropertyKwikCheck.Tests;

public class LeadMapperTests
{
    [Fact]
    public void Maps_columns_to_camelCase_api_keys()
    {
        var lead = new Lead
        {
            Id = 4812,
            ReqId = "4WRP04812",
            AssetFamily = "property",
            PropertyType = "Residential",
            Stage = "qc",
            LenderName = "HDFC Bank Ltd",
            ValuatorName = "Rahul Mehta",
            ExecName = "Meena Patil",
            AssignedOn = new DateTime(2026, 5, 18),
            LeadDate = new DateTime(2026, 5, 12),
            ReportData = """{"reportType":"Property Inspection"}""",
        };

        var dto = LeadMapper.ToDto(lead);

        dto.Type.Should().Be("property");          // asset_family -> type
        dto.Ptype.Should().Be("Residential");      // property_type -> ptype
        dto.Lender.Should().Be("HDFC Bank Ltd");   // lender_name -> lender
        dto.Valuator.Should().Be("Rahul Mehta");
        dto.Exec.Should().Be("Meena Patil");
        dto.AssignedOn.Should().Be("18/5/2026");   // d/M/yyyy
        dto.LeadDate.Should().Be("2026-05-12");    // ISO
        dto.Data!["reportType"]!.GetValue<string>().Should().Be("Property Inspection");
    }

    [Fact]
    public void Null_data_yields_null_object()
        => LeadMapper.ParseData(null).Should().BeNull();
}

public class JsonMergeTests
{
    [Fact]
    public void Top_level_keys_from_patch_overwrite_existing()
    {
        var existing = JsonNode.Parse("""{"a":"1","b":"2"}""")!.AsObject();
        var patch = JsonNode.Parse("""{"b":"9","c":"3"}""")!.AsObject();

        var merged = JsonMerge.Merge(existing, patch);

        merged["a"]!.GetValue<string>().Should().Be("1");
        merged["b"]!.GetValue<string>().Should().Be("9");
        merged["c"]!.GetValue<string>().Should().Be("3");
    }

    [Fact]
    public void Null_existing_returns_clone_of_patch()
    {
        var patch = JsonNode.Parse("""{"x":"1"}""")!.AsObject();
        JsonMerge.Merge(null, patch)["x"]!.GetValue<string>().Should().Be("1");
    }
}

public class InrTests
{
    [Theory]
    [InlineData(480000, "₹ 4,80,000")]
    [InlineData(21540000, "₹ 2,15,40,000")]
    [InlineData(0, "₹ 0")]
    public void Formats_indian_grouping(long amount, string expected)
        => Inr.Format(amount).Should().Be(expected);
}
