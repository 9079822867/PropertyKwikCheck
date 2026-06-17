namespace PropertyKwikCheck.Core.Rbac;

/// <summary>
/// Maps the user's <c>UserType</c> (20 org user types, spec-provided) onto the
/// workflow capability matrix (spec §9.1) and a row-visibility <see cref="Scope"/>.
/// This is the bridge between the user's identity model (Roles + UserTypes) and the
/// functional permissions the spec describes for its 9 abstract roles.
/// </summary>
public static class RbacPolicy
{
    public sealed record Policy(IReadOnlySet<Capability> Capabilities, Scope Scope)
    {
        public bool Can(Capability cap) => Capabilities.Contains(cap);
    }

    // UserType ids (PropertyDB.UserTypes). CompanyTypeId aligns with Roles.Id:
    // 1 Client · 2 RO · 3 Internal · 4 Cando.
    public static class UserTypeId
    {
        public const int ClientExecutive = 1;
        public const int ClientHubHead = 2;
        public const int ClientStateHead = 3;
        public const int ClientZonalHead = 4;
        public const int ClientNationalManager = 5;
        public const int ClientAdmin = 6;
        public const int RoAdmin = 7;
        public const int RoValuators = 8;
        public const int StateCoordinator = 9;
        public const int StateHead = 10;
        public const int QcManager = 11;
        public const int PricingManager = 12;
        public const int ZonalHead = 13;
        public const int NationalHead = 14;
        public const int BusinessHead = 15;
        public const int Admin = 16;
        public const int CandoValuator = 17;
        public const int CandoAdmin = 18;
        public const int SuperAdmin = 19;
        public const int CandoExecutive = 20;
    }

    private static readonly IReadOnlySet<Capability> AllCaps =
        new HashSet<Capability>(Enum.GetValues<Capability>());

    private static IReadOnlySet<Capability> Caps(params Capability[] caps) => new HashSet<Capability>(caps);

    // Least-privilege fallback for an unmapped user type.
    private static readonly Policy Default = new(Caps(Capability.ViewLeads), Scope.OwnLeads);

    private static readonly Dictionary<int, Policy> Map = Build();

    private static Dictionary<int, Policy> Build()
    {
        // Internal admins — full access.
        var admin = new Policy(AllCaps, Scope.All);

        // Lead Manager profile (intake/assign/reject/analytics across all leads).
        var leadManager = new Policy(Caps(
            Capability.ViewLeads, Capability.CreateLead, Capability.EditReport, Capability.UploadFiles,
            Capability.AssignReassign, Capability.RejectDuplicate, Capability.ManageYard,
            Capability.ViewAnalytics, Capability.DeleteLead), Scope.All);

        // Lead Manager without create/edit (oversight + analytics).
        var oversight = new Policy(Caps(
            Capability.ViewLeads, Capability.AssignReassign, Capability.RejectDuplicate,
            Capability.ManageYard, Capability.ViewAnalytics, Capability.DeleteLead), Scope.All);

        var qc = new Policy(Caps(
            Capability.ViewLeads, Capability.EditReport, Capability.QcApproveHold,
            Capability.RejectDuplicate, Capability.ViewAnalytics), Scope.All);

        var pricing = new Policy(Caps(
            Capability.ViewLeads, Capability.EditReport, Capability.PricingSignoff,
            Capability.ViewBilling, Capability.ViewAnalytics), Scope.All);

        var authoriser = new Policy(Caps(
            Capability.ViewLeads, Capability.AuthoriseIssue, Capability.ViewBilling,
            Capability.ViewAnalytics), Scope.All);

        // Field valuer — own leads only.
        var valuer = new Policy(Caps(
            Capability.ViewLeads, Capability.EditReport, Capability.UploadFiles,
            Capability.SubmitForQc, Capability.ManageYard), Scope.OwnLeads);

        // RO/Cando company manager — assign within company, manage yard.
        var roManager = new Policy(Caps(
            Capability.ViewLeads, Capability.CreateLead, Capability.AssignReassign,
            Capability.RejectDuplicate, Capability.UploadFiles, Capability.EditReport,
            Capability.ManageYard, Capability.ViewAnalytics), Scope.All);

        // Bank/client coordinator — own company only.
        var coordinator = new Policy(Caps(
            Capability.ViewLeads, Capability.CreateLead, Capability.ViewAnalytics), Scope.OwnCompany);

        return new Dictionary<int, Policy>
        {
            [UserTypeId.SuperAdmin] = admin,
            [UserTypeId.Admin] = admin,
            [UserTypeId.StateCoordinator] = leadManager,
            [UserTypeId.StateHead] = oversight,
            [UserTypeId.ZonalHead] = oversight,
            [UserTypeId.QcManager] = qc,
            [UserTypeId.PricingManager] = pricing,
            [UserTypeId.NationalHead] = authoriser,
            [UserTypeId.BusinessHead] = authoriser,
            [UserTypeId.RoAdmin] = roManager,
            [UserTypeId.RoValuators] = valuer,
            [UserTypeId.CandoValuator] = valuer,
            [UserTypeId.CandoAdmin] = roManager,
            [UserTypeId.ClientExecutive] = coordinator,
            [UserTypeId.ClientHubHead] = coordinator,
            [UserTypeId.ClientStateHead] = coordinator,
            [UserTypeId.ClientZonalHead] = coordinator,
            [UserTypeId.ClientNationalManager] = coordinator,
            [UserTypeId.ClientAdmin] = coordinator,
            [UserTypeId.CandoExecutive] = coordinator,
        };
    }

    public static Policy For(int userTypeId) => Map.GetValueOrDefault(userTypeId, Default);

    public static bool Can(int userTypeId, Capability cap) => For(userTypeId).Can(cap);

    public static Scope ScopeFor(int userTypeId) => For(userTypeId).Scope;
}
