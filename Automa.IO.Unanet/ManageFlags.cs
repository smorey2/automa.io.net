using System;

namespace Automa.IO.Unanet
{
    [Flags] // XX._TPJO.O
    public enum ManageFlags
    {
        None = 0x0,
        //Default = Alternate | Export,
        Default = AllEntity | Export | Sync | Sync2,
        AllEntity = OrganizationAll | ProjectAll | PersonAll | TimeInvoiceAll,

        // OPERATIONS
        Export = 0x1, Sync = 0x2, Sync2 = 0x4,

        // GROUPS
        // 0x01 - Organization
        OrganizationMask = 0x010000F0, OrganizationAll = Organization | CustomerProfile | OrganizationAddress | OrganizationContact | VendorProfile,
        Organization = 0x01000010,
        CustomerProfile = 0x01000020,
        OrganizationAddress = 0x01000040,
        OrganizationContact = 0x01000080,
        VendorProfile = 0x01000100,
        // changed
        OrganizationChanged = Organization | Export,
        CustomerProfileChanged = CustomerProfile | Export,
        OrganizationAddressChanged = OrganizationAddress | Export,
        OrganizationContactChanged = OrganizationContact | Export,
        VendorProfileChanged = VendorProfile | Export,

        // 0x06 - Project
        ProjectMask = 0x06000F00, ProjectAll = Project | ProjectLaborCategory | Task | FixedPrice | Assignment | ProjectAdministrator | ProjectInvoiceSetup,
        Project = 0x02000100,
        Task = 0x02000200,
        ProjectLaborCategory = 0x02000400,
        FixedPrice = 0x02000800,
        Assignment = 0x04000100,
        ProjectAdministrator = 0x04000200,
        ProjectInvoiceSetup = 0x04000400,
        // changed
        ProjectChanged = Project | Export,
        TaskChanged = Task | Export,
        FixedPriceChanged = FixedPrice | Export,
        AssignmentChanged = Assignment | Export,
        ProjectAdministratorChanged = ProjectAdministrator | Export,
        ProjectInvoiceSetupChanged = ProjectInvoiceSetup | Export,
        ProjectLaborCategoryChanged = ProjectLaborCategory | Export,

        // 0x08 - Person
        PersonMask = 0x0800F000, PersonAll = Person | Alternate | PersonAccess, /*ApprovalGroups, */
        Person = 0x08001000,
        Alternate = 0x08002000,
        PersonAccess = 0x08004000,
        ApprovalGroups = 0x08008000,
        // changed
        PersonChanged = Person | Export,
        AlternateChanged = Alternate | Export,
        PersonAccessChanged = PersonAccess | Export,
        ApprovalGroupChanged = ApprovalGroups | Export,

        // 0x10 - Entity:Time
        TimeInvoiceMask = 0x100F0000, TimeInvoiceAll = Time | Invoice,
        Time = 0x10010000,
        Invoice = 0x10020000,
    }
}