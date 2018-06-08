using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace SV.Domain.Code
{
	public class SvConstraint
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Abbreviation { get; set; }
	}

	public class SvConstraints
	{
		private static SvConstraints instance = new SvConstraints();

		protected SvConstraints() { }

		public static SvConstraints Instance { get { return instance; } }
		public const string BaseCompanyName = "Insurance Batch Control";
		public const string UserRoleAdmin = "Admin";
		public const string UserRoleCompanyAdmin = "Company Admin";
		public const string UserRoleMember = "Member";
		public const string UserAuthorizeAll = UserRoleAdmin + ", " + UserRoleCompanyAdmin + ", " + UserRoleMember;
		public const string UserAuthorizeAdmin = UserRoleAdmin + ", " + UserRoleCompanyAdmin;


		public virtual void Init()
		{
			// remember instance so Sv can access too
			instance = this;
			SvRoleEnumAdmin = 1;
			SvRoleEnumCompanyAdmin = 2;
			SvRoleEnumMember = 3;
			SvRoleEnumClient = 4;

			if(_SvRoles == null)
			{
				_SvRoles = new List<SvConstraint>()
					{
						new SvConstraint() { Id = SvRoleEnumAdmin, Name = UserRoleAdmin },
						new SvConstraint() { Id = SvRoleEnumCompanyAdmin, Name = UserRoleCompanyAdmin },
						new SvConstraint() { Id = SvRoleEnumMember, Name = UserRoleMember },
					};
			}
		}

		public int SvRoleEnumAdmin;
		public int SvRoleEnumCompanyAdmin;
		public int SvRoleEnumMember;
		public int SvRoleEnumClient;
		protected List<SvConstraint> _SvRoles;

		public virtual IEnumerable<SvConstraint> SvRoles
		{
			get
			{
				return _SvRoles;
			}
		}

		public string SvRolesAdmin { get { return SvRoleName(SvRoleEnumAdmin); } }

		public string SvRolesCompanyAdmin { get { return SvRoleName(SvRoleEnumCompanyAdmin); } }

		public string SvRolesMember { get { return SvRoleName(SvRoleEnumMember); } }

		public string SvRolesClient { get { return SvRoleName(SvRoleEnumClient); } }

		public string SvRoleName(int roleEnum)
		{
			try
			{
				SvConstraint c = SvRoles.Single(r => r.Id == roleEnum);
				if(c != null)
					return c.Name;
			}
			catch(Exception)
			{
			}

			return "Unknown role: " + roleEnum;
		}

		public IEnumerable<SvConstraint> SvRolesForRole(string currentRole)
		{
			if(currentRole == SvRolesClient)
				return SvRoles.Where(x => x.Id <= SvRoleEnumClient);
			else if(currentRole == SvRolesMember)
				return SvRoles.Where(x => x.Id <= SvRoleEnumMember);
			else if(currentRole == SvRolesCompanyAdmin)
				return SvRoles.Where(x => x.Id <= SvRoleEnumCompanyAdmin);
			else
				return SvRoles;
		}

		public IEnumerable<SvConstraint> SvRolesForRole(int currentRoleEnum)
		{
			if(currentRoleEnum == SvRoleEnumMember)
				return SvRoles.Where(x => x.Id == SvRoleEnumMember);
			else if(currentRoleEnum == SvRoleEnumCompanyAdmin)
				return SvRoles.Where(x => x.Id == SvRoleEnumMember || x.Id == SvRoleEnumCompanyAdmin);
			else
				return SvRoles;
		}

		public IEnumerable<SvConstraint> SvUserStatus = new List<SvConstraint>()
			{
				new SvConstraint() { Id = 0, Name = "All" },
				new SvConstraint() { Id = 1, Name = "Invited" },
				new SvConstraint() { Id = 2, Name = "Active" },
				new SvConstraint() { Id = 3, Name = "Disabled" },
			};

		public string SvUserStatusAll { get { return SvUserStatus.ElementAt(0).Name; } }
		public string SvUserStatusInvited { get { return SvUserStatus.ElementAt(1).Name; } }
		public string SvUserStatusActive { get { return SvUserStatus.ElementAt(2).Name; } }
		public string SvUserStatusDisabled { get { return SvUserStatus.ElementAt(3).Name; } }
		public IEnumerable<SvConstraint> SvUserStatusActiveDisabled
		{
			get { return SvUserStatus.Where(x => x.Name == SvUserStatusActive || x.Name == SvUserStatusDisabled); }
		}

		public IEnumerable<SvConstraint> SvYesNo = new List<SvConstraint>()
		{
			new SvConstraint() { Id = 0, Name = "No" },
			new SvConstraint() { Id = 1, Name = "Yes" },
		};
		public string SvYesNoNo { get { return SvYesNo.ElementAt(0).Name; } }
		public string SvYesNoYes { get { return SvYesNo.ElementAt(1).Name; } }

		public IEnumerable<SvConstraint> SvGender = new List<SvConstraint>()
		{
			new SvConstraint() { Id = 0, Name = "Male" },
			new SvConstraint() { Id = 1, Name = "Female" },
		};
		public string SvGenderMale { get { return SvGender.ElementAt(0).Name; } }
		public string SvGenderFemale { get { return SvGender.ElementAt(1).Name; } }

		public string Lookup(IEnumerable<SvConstraint> list, int value)
		{
			return list.Single(i => i.Id == value).Name;
		}

		public int LookupIndex(IEnumerable<SvConstraint> list, int value)
		{
			int i = 0;
			foreach(var l in list)
			{
				if(value == l.Id)
					return i;
				i++;
			}
			// return invalid
			return list.Count();
		}
		public int LookupIndexByName(IEnumerable<SvConstraint> list, string name)
		{
			int i = 0;
			foreach(var l in list)
			{
				if(name.Trim() == l.Name.Trim())
					return i;
				i++;
			}
			// return invalid
			return list.Count();
		}
        public const int InstructionTypeWindowTitle = 1;
       
        public const int InstructionTypeFindDestination = 2;
        public const int InstructionTypeGeneralInstruction = 3;
        public const int InstructionTypeFindSource = 4;
        public const int InstructionTypeOpenPath = 5;
        public const int InstructionTypeClosePath = 6;
        public const int InstructionTypeQuantityStart = 7;
        public const int InstructionTypeDestinationStartWeight = 8;
        public const int InstructionTypeTotalAdditionalQuantity = 9;
        public const int InstructionTypeDestinationEndWeight = 10;
        public const int InstructionTypeCompleteAction = 11;
        public const int InstructionTypeVerifyMaterial = 12;
        public const int InstructionTypeContainerScan = 13;
        public const int UserDataEntry1 = 14;
        public const int InstructionTypeScaleCheck = 15;
        public const int AdditionQtyVerification = 16;
        public const int SourceStartWeight = 17;
        public const int SourceEndWeight = 18;
        public const int SelectDestination = 19;
    }
}
