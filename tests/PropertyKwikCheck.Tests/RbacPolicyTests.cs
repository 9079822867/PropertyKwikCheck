using FluentAssertions;
using PropertyKwikCheck.Core.Rbac;

namespace PropertyKwikCheck.Tests;

public class RbacPolicyTests
{
    [Fact]
    public void SuperAdmin_has_every_capability_and_global_scope()
    {
        var p = RbacPolicy.For(RbacPolicy.UserTypeId.SuperAdmin);
        p.Scope.Should().Be(Scope.All);
        foreach (var cap in Enum.GetValues<Capability>())
            p.Can(cap).Should().BeTrue($"super admin should have {cap}");
    }

    [Fact]
    public void RoValuator_is_scoped_to_own_leads_and_cannot_manage_users()
    {
        var p = RbacPolicy.For(RbacPolicy.UserTypeId.RoValuators);
        p.Scope.Should().Be(Scope.OwnLeads);
        p.Can(Capability.EditReport).Should().BeTrue();
        p.Can(Capability.SubmitForQc).Should().BeTrue();
        p.Can(Capability.ManageUsers).Should().BeFalse();
        p.Can(Capability.QcApproveHold).Should().BeFalse();
    }

    [Fact]
    public void ClientExecutive_is_scoped_to_own_company()
    {
        var p = RbacPolicy.For(RbacPolicy.UserTypeId.ClientExecutive);
        p.Scope.Should().Be(Scope.OwnCompany);
        p.Can(Capability.ViewLeads).Should().BeTrue();
        p.Can(Capability.CreateLead).Should().BeTrue();
        p.Can(Capability.AssignReassign).Should().BeFalse();
    }

    [Fact]
    public void QcManager_can_approve_hold_but_not_price()
    {
        var p = RbacPolicy.For(RbacPolicy.UserTypeId.QcManager);
        p.Can(Capability.QcApproveHold).Should().BeTrue();
        p.Can(Capability.PricingSignoff).Should().BeFalse();
    }

    [Fact]
    public void Unknown_usertype_falls_back_to_least_privilege()
    {
        var p = RbacPolicy.For(9999);
        p.Scope.Should().Be(Scope.OwnLeads);
        p.Can(Capability.ViewLeads).Should().BeTrue();
        p.Can(Capability.CreateLead).Should().BeFalse();
    }
}
