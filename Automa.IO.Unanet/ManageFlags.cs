using System;

namespace Automa.IO.Unanet
{
    [Flags] // XX._TPJO.O
    public enum ManageFlags
    {
        None = 0x0,
        Default = AllEntity | Export | Sync,
        AllEntity = OrganizationAll | ProjectAll | PersonAll | TimeInvoiceAll,
        //AllEntity = OrganizationContact,

        // OPERATIONS
        Export = 0x1, Sync = 0x2,

        // GROUPS
        // 0x01 - Organization
        OrganizationMask = 0x010000F0, OrganizationAll = Organization | CustomerProfile | OrganizationAddress, // | OrganizationContact | OrganizationContactEmail,
        Organization = 0x01000010,
        CustomerProfile = 0x01000020,
        OrganizationAddress = 0x01000040,
        OrganizationContact = 0x01000080,
        OrganizationContactAddress = 0x01000100,
        OrganizationContactEmail = 0x01000200,
        OrganizationContactPhone = 0x01000400,
        // changed
        OrganizationChanged = Organization | Export,
        CustomerProfileChanged = CustomerProfile | Export,
        OrganizationAddressChanged = OrganizationAddress | Export,
        OrganizationContactChanged = OrganizationContact | Export,
        OrganizationContactAddressChanged = OrganizationContactAddress | Export,
        OrganizationContactEmailChanged = OrganizationContactEmail | Export,
        OrganizationContactPhoneChanged = OrganizationContactPhone | Export,

        // 0x06 - Project
        ProjectMask = 0x06000F00, ProjectAll = Project | ProjectLaborCategory | Task /*| FixedPrice*/| Assignment | ProjectAdministrator | ProjectInvoiceSetup,
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
        PersonMask = 0x0800F000, PersonAll = Person | Alternate, /*ApprovalGroups, */
        Person = 0x08001000,
        Alternate = 0x08002000,
        ApprovalGroups = 0x08004000,
        // changed
        PersonChanged = Person | Export,
        AlternateChanged = Alternate | Export,
        ApprovalGroupChanged = ApprovalGroups | Export,

        // 0x10 - Entity:Time
        TimeInvoiceMask = 0x100F0000, TimeInvoiceAll = Time | Invoice,
        Time = 0x10010000,
        Invoice = 0x10020000,
    }
}