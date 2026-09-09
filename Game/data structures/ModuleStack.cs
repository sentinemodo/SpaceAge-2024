using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SpaceAge
{
	public partial class ModuleStack : NamedObject, IHolder, IItemStacksHolder, IOfferent, IReporting, IEventReporting, IEffectable, IMoveable
	{
		public const int NameLength = NamedObject.MaxNameLength;

        public ModuleStack(IHolder parent, string name)
            : base(name)
        {
            if (ModuleStack.All.ContainsKey(name))
                throw new Exception("Modulestack with name [" + name + "] already exists");

            ModuleStack.All.Add(this.name, this);
            this.parent = parent;
        }

		public ModuleStack(IHolder parent, Faction owner, ModuleType type, string name)
			: base(name)
		{
			if (ModuleStack.All.ContainsKey(name))
				throw new Exception("Modulestack with name [" + name + "] already exists");

			ModuleStack.All.Add(this.name, this);

			this.alias = this.name; 
			this.parent = parent;
			this.owner = owner;
			this.moduleType = type;			
		}

		public ModuleStack(IHolder parent, Faction owner, ModuleType type)
			: base("")
		{
			this.name = this.GenerateUniqueModuleStackIdentifier();
			ModuleStack.All.Add(this.name, this);

			this.alias = this.name;
			this.parent = parent;
			this.owner = owner;
			this.moduleType = type;			
		}

		public ModuleStack(IHolder parent, Faction owner)
			: base("")
		{
			this.name = this.GenerateUniqueModuleStackIdentifier();
			ModuleStack.All.Add(this.name, this);

			this.parent = parent;
			this.owner = owner;
			this.moduleType = null;
			this.modules = new Modules();
		}

		public ModuleStack(Faction owner, string name)
			: base("")
		{
            if (name.StartsWith("new"))
            {
                // okay if it's a new alias we need to create new identifier
				string generatedRandomIdentifier = this.GenerateUniqueModuleStackIdentifier();

				this.name = generatedRandomIdentifier;
            } else
            {
                // if it's not a new alias we need to check if we don't try to create a duplicate
                if (ModuleStack.All.ContainsKey(name))
                    throw new Exception("Modulestack with name [" + name + "] already exists");

                this.name = name;
            }

            while (ModuleStack.All.ContainsKey(this.name))
            {
                this.name = this.GenerateUniqueModuleStackIdentifier();
            }

            ModuleStack.All.Add(this.name, this);

            this.alias = name;

            this.owner = owner;
			this.moduleType = null;
			this.modules = new Modules();
		}

		public static ModuleStacks All = new ModuleStacks();
	}
}
