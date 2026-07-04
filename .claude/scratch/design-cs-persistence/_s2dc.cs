using System;
using System.Collections.Generic;
using System.Linq;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Type;

namespace FlowtideDotNet.Substrait.Conversion;

internal class SubstraitToDifferentialComputeVisitor : RelationVisitor<Relation, object>
{
	private readonly bool addWrite;

	private readonly string tableName;

	private readonly List<string> primaryKeys;

	public SubstraitToDifferentialComputeVisitor(bool addWrite, string tableName, List<string> primaryKeys)
	{
		this.addWrite = addWrite;
		this.tableName = tableName;
		this.primaryKeys = primaryKeys;
	}

	public override Relation VisitProjectRelation(ProjectRelation projectRelation, object state)
	{
		projectRelation.Input = Visit(projectRelation.Input, state);
		return projectRelation;
	}

	public override Relation VisitRootRelation(RootRelation rootRelation, object state)
	{
		Relation relation = Visit(rootRelation.Input, state);
		Relation input = relation;
		if (addWrite)
		{
			List<SubstraitBaseType> list = new List<SubstraitBaseType>();
			foreach (string name in rootRelation.Names)
			{
				bool nullable = true;
				if (primaryKeys.Contains<string>(name, StringComparer.InvariantCultureIgnoreCase))
				{
					nullable = false;
				}
				list.Add(new AnyType
				{
					Nullable = nullable
				});
			}
			input = new WriteRelation
			{
				Input = relation,
				TableSchema = new NamedStruct
				{
					Names = rootRelation.Names,
					Struct = new Struct
					{
						Types = list
					}
				},
				NamedObject = new NamedTable
				{
					Names = new List<string> { tableName }
				}
			};
		}
		rootRelation.Input = input;
		return rootRelation;
	}

	public override Relation VisitWriteRelation(WriteRelation writeRelation, object state)
	{
		writeRelation.Input = Visit(writeRelation.Input, state);
		return writeRelation;
	}

	public override Relation VisitSetRelation(SetRelation setRelation, object state)
	{
		for (int i = 0; i < setRelation.Inputs.Count; i++)
		{
			setRelation.Inputs[i] = Visit(setRelation.Inputs[i], state);
		}
		return setRelation;
	}

	public override Relation VisitFilterRelation(FilterRelation filterRelation, object state)
	{
		if (filterRelation.Input is ReadRelation readRelation)
		{
			readRelation.Filter = filterRelation.Condition;
			return Visit(filterRelation.Input, state);
		}
		return base.VisitFilterRelation(filterRelation, state);
	}

	public override Relation VisitReadRelation(ReadRelation readRelation, object state)
	{
		int num = -1;
		if (readRelation.BaseSchema.Struct == null)
		{
			throw new NotSupportedException("Struct must be defined with types");
		}
		for (int i = 0; i < readRelation.BaseSchema.Struct.Types.Count; i++)
		{
			if (!readRelation.BaseSchema.Struct.Types[i].Nullable)
			{
				num = i;
			}
		}
		if (num == -1)
		{
			throw new NotSupportedException("One column must be not nullable");
		}
		NormalizationRelation result = new NormalizationRelation
		{
			Filter = readRelation.Filter,
			Input = readRelation,
			KeyIndex = new List<int> { num }
		};
		readRelation.Filter = null;
		return result;
	}

	public override Relation VisitJoinRelation(JoinRelation joinRelation, object state)
	{
		joinRelation.Left = Visit(joinRelation.Left, state);
		joinRelation.Right = Visit(joinRelation.Right, state);
		return joinRelation;
	}

	public override Relation VisitNormalizationRelation(NormalizationRelation normalizationRelation, object state)
	{
		return normalizationRelation;
	}

	public override Relation VisitVirtualTableReadRelation(VirtualTableReadRelation virtualTableReadRelation, object state)
	{
		return base.VisitVirtualTableReadRelation(virtualTableReadRelation, state);
	}
}
