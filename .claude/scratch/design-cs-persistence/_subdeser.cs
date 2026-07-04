using System;
using System.Collections.Generic;
using System.Linq;
using FlowtideDotNet.Substrait.CustomProtobuf;
using FlowtideDotNet.Substrait.Exceptions;
using FlowtideDotNet.Substrait.Expressions;
using FlowtideDotNet.Substrait.Expressions.IfThen;
using FlowtideDotNet.Substrait.Expressions.Literals;
using FlowtideDotNet.Substrait.Relations;
using FlowtideDotNet.Substrait.Type;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Substrait.Protobuf;

namespace FlowtideDotNet.Substrait;

public class SubstraitDeserializer
{
	private sealed class ExpressionDeserializerImpl
	{
		private readonly Dictionary<uint, string> idToFunctionLookup = new Dictionary<uint, string>();

		private readonly Dictionary<uint, string> idToUserDefinedType = new Dictionary<uint, string>();

		public ExpressionDeserializerImpl(global::Substrait.Protobuf.Plan plan)
		{
			foreach (SimpleExtensionDeclaration extension in plan.Extensions)
			{
				if (extension.MappingTypeCase == SimpleExtensionDeclaration.MappingTypeOneofCase.ExtensionType)
				{
					idToUserDefinedType.Add(extension.ExtensionType.TypeAnchor, extension.ExtensionType.Name);
					continue;
				}
				if (extension.MappingTypeCase == SimpleExtensionDeclaration.MappingTypeOneofCase.ExtensionFunction)
				{
					uint functionAnchor = extension.ExtensionFunction.FunctionAnchor;
					string uri = ((IEnumerable<SimpleExtensionURI>)plan.ExtensionUris).First((SimpleExtensionURI x) => x.ExtensionUriAnchor == extension.ExtensionFunction.ExtensionUriReference).Uri;
					uri = uri.Substring(uri.LastIndexOf('/'));
					int num = extension.ExtensionFunction.Name.IndexOf(':');
					string text = ((num != -1) ? extension.ExtensionFunction.Name.Substring(0, num) : extension.ExtensionFunction.Name);
					idToFunctionLookup.Add(functionAnchor, uri + ":" + text);
					continue;
				}
				throw new NotImplementedException(extension.MappingTypeCase.ToString());
			}
		}

		internal SubstraitBaseType GetType(global::Substrait.Protobuf.Type type, ref Span<string> names)
		{
			switch (type.KindCase)
			{
			case global::Substrait.Protobuf.Type.KindOneofCase.String:
				return new StringType
				{
					Nullable = (type.String.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.I32:
				return new Int32Type
				{
					Nullable = (type.I32.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.Date:
				return new DateType
				{
					Nullable = (type.Date.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.Fp64:
				return new Fp64Type
				{
					Nullable = (type.Fp64.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.I64:
				return new Int64Type
				{
					Nullable = (type.I64.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.Bool:
				return new BoolType
				{
					Nullable = (type.Bool.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.Fp32:
				return new Fp32Type
				{
					Nullable = (type.Fp32.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.UserDefined:
			{
				if (idToUserDefinedType.TryGetValue(type.UserDefined.TypeReference, out string value))
				{
					if (value.Equals("any", StringComparison.OrdinalIgnoreCase))
					{
						return new AnyType();
					}
					throw new NotImplementedException("User defined type not implemented " + value);
				}
				throw new InvalidOperationException($"User defined type not found {type.UserDefined.TypeReference}");
			}
			case global::Substrait.Protobuf.Type.KindOneofCase.Struct:
			{
				List<string> list = new List<string>();
				List<SubstraitBaseType> list2 = new List<SubstraitBaseType>();
				for (int i = 0; i < type.Struct.Types_.Count; i++)
				{
					if (names.Length > 0)
					{
						string item = names[0];
						names = names.Slice(1);
						SubstraitBaseType type4 = GetType(type.Struct.Types_[i], ref names);
						list.Add(item);
						list2.Add(type4);
						continue;
					}
					throw new InvalidOperationException("Not enough names for struct fields");
				}
				return new FlowtideDotNet.Substrait.Type.NamedStruct
				{
					Names = list,
					Struct = new Struct
					{
						Types = list2
					}
				};
			}
			case global::Substrait.Protobuf.Type.KindOneofCase.List:
				return new ListType(GetType(type.List.Type, ref names))
				{
					Nullable = (type.List.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.Map:
			{
				SubstraitBaseType type2 = GetType(type.Map.Key, ref names);
				SubstraitBaseType type3 = GetType(type.Map.Value, ref names);
				return new MapType(type2, type3)
				{
					Nullable = (type.Map.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			}
			case global::Substrait.Protobuf.Type.KindOneofCase.TimestampTz:
				return new TimestampType
				{
					Nullable = (type.TimestampTz.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			case global::Substrait.Protobuf.Type.KindOneofCase.Binary:
				return new BinaryType
				{
					Nullable = (type.Binary.Nullability != global::Substrait.Protobuf.Type.Types.Nullability.Required)
				};
			default:
				throw new NotImplementedException($"Type is not yet implemented {type.KindCase}");
			}
		}

		internal Struct ParseStruct(global::Substrait.Protobuf.Type.Types.Struct str)
		{
			Struct obj = new Struct
			{
				Types = new List<SubstraitBaseType>()
			};
			foreach (global::Substrait.Protobuf.Type item in str.Types_)
			{
				Span<string> names = Span<string>.Empty;
				obj.Types.Add(GetType(item, ref names));
			}
			return obj;
		}

		internal FlowtideDotNet.Substrait.Type.NamedStruct ParseNamedStruct(global::Substrait.Protobuf.NamedStruct namedStruct)
		{
			List<string> list = new List<string>();
			Struct obj = null;
			if (namedStruct.Struct != null)
			{
				List<SubstraitBaseType> list2 = new List<SubstraitBaseType>();
				Span<string> names = ((IEnumerable<string>)namedStruct.Names).ToArray().AsSpan();
				for (int i = 0; i < namedStruct.Struct.Types_.Count; i++)
				{
					string item = names[0];
					names = names.Slice(1);
					SubstraitBaseType type = GetType(namedStruct.Struct.Types_[i], ref names);
					list.Add(item);
					list2.Add(type);
				}
				obj = new Struct
				{
					Types = list2
				};
			}
			return new FlowtideDotNet.Substrait.Type.NamedStruct
			{
				Names = list,
				Struct = obj
			};
		}

		public FlowtideDotNet.Substrait.Expressions.TableFunction VisitTableFunction(FlowtideDotNet.Substrait.CustomProtobuf.TableFunction tableFunction)
		{
			if (!idToFunctionLookup.TryGetValue(tableFunction.FunctionReference, out string value))
			{
				throw new NotImplementedException();
			}
			string extensionUri = value.Substring(0, value.IndexOf(':'));
			string extensionName = value.Substring(value.IndexOf(':') + 1);
			FlowtideDotNet.Substrait.Type.NamedStruct tableSchema = ParseNamedStruct(tableFunction.TableSchema);
			FlowtideDotNet.Substrait.Expressions.TableFunction tableFunction2 = new FlowtideDotNet.Substrait.Expressions.TableFunction
			{
				ExtensionName = extensionName,
				ExtensionUri = extensionUri,
				TableSchema = tableSchema,
				Arguments = new List<FlowtideDotNet.Substrait.Expressions.Expression>()
			};
			if (tableFunction.Arguments != null && tableFunction.Arguments.Count > 0)
			{
				foreach (FunctionArgument argument in tableFunction.Arguments)
				{
					tableFunction2.Arguments.Add(VisitExpression(argument.Value));
				}
			}
			return tableFunction2;
		}

		public FlowtideDotNet.Substrait.Expressions.Expression VisitExpression(global::Substrait.Protobuf.Expression expression)
		{
			return expression.RexTypeCase switch
			{
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.ScalarFunction => VisitScalarFunction(expression.ScalarFunction), 
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.Selection => VisitFieldReference(expression.Selection), 
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.Literal => VisitLiteral(expression.Literal), 
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.IfThen => VisitIfThen(expression.IfThen), 
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.Cast => VisitCast(expression.Cast), 
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.Nested => VisitNested(expression.Nested), 
				global::Substrait.Protobuf.Expression.RexTypeOneofCase.SingularOrList => VisitSingularOrList(expression.SingularOrList), 
				_ => throw new NotImplementedException(), 
			};
		}

		public StructExpression VisitStruct(global::Substrait.Protobuf.Expression.Types.Nested.Types.Struct structExpr)
		{
			List<FlowtideDotNet.Substrait.Expressions.Expression> list = new List<FlowtideDotNet.Substrait.Expressions.Expression>();
			foreach (global::Substrait.Protobuf.Expression field in structExpr.Fields)
			{
				list.Add(VisitExpression(field));
			}
			return new StructExpression
			{
				Fields = list
			};
		}

		public FlowtideDotNet.Substrait.Expressions.Expression VisitSingularOrList(global::Substrait.Protobuf.Expression.Types.SingularOrList singularOrList)
		{
			FlowtideDotNet.Substrait.Expressions.Expression value = VisitExpression(singularOrList.Value);
			List<FlowtideDotNet.Substrait.Expressions.Expression> list = new List<FlowtideDotNet.Substrait.Expressions.Expression>();
			for (int i = 0; i < singularOrList.Options.Count; i++)
			{
				list.Add(VisitExpression(singularOrList.Options[i]));
			}
			return new SingularOrListExpression
			{
				Options = list,
				Value = value
			};
		}

		public FlowtideDotNet.Substrait.Expressions.Expression VisitNested(global::Substrait.Protobuf.Expression.Types.Nested nested)
		{
			return nested.NestedTypeCase switch
			{
				global::Substrait.Protobuf.Expression.Types.Nested.NestedTypeOneofCase.List => VisitListNestedExpression(nested.List), 
				global::Substrait.Protobuf.Expression.Types.Nested.NestedTypeOneofCase.Map => VisitMapNestedExpression(nested.Map), 
				global::Substrait.Protobuf.Expression.Types.Nested.NestedTypeOneofCase.Struct => VisitStruct(nested.Struct), 
				_ => throw new NotImplementedException(), 
			};
		}

		public MapNestedExpression VisitMapNestedExpression(global::Substrait.Protobuf.Expression.Types.Nested.Types.Map map)
		{
			MapNestedExpression mapNestedExpression = new MapNestedExpression
			{
				KeyValues = new List<KeyValuePair<FlowtideDotNet.Substrait.Expressions.Expression, FlowtideDotNet.Substrait.Expressions.Expression>>()
			};
			foreach (global::Substrait.Protobuf.Expression.Types.Nested.Types.Map.Types.KeyValue keyValue in map.KeyValues)
			{
				FlowtideDotNet.Substrait.Expressions.Expression key = VisitExpression(keyValue.Key);
				FlowtideDotNet.Substrait.Expressions.Expression value = VisitExpression(keyValue.Value);
				mapNestedExpression.KeyValues.Add(new KeyValuePair<FlowtideDotNet.Substrait.Expressions.Expression, FlowtideDotNet.Substrait.Expressions.Expression>(key, value));
			}
			return mapNestedExpression;
		}

		public ListNestedExpression VisitListNestedExpression(global::Substrait.Protobuf.Expression.Types.Nested.Types.List list)
		{
			List<FlowtideDotNet.Substrait.Expressions.Expression> list2 = new List<FlowtideDotNet.Substrait.Expressions.Expression>();
			foreach (global::Substrait.Protobuf.Expression value in list.Values)
			{
				FlowtideDotNet.Substrait.Expressions.Expression expression = VisitExpression(value);
				if (expression == null)
				{
					throw new InvalidOperationException("Expression in list cannot be null");
				}
				list2.Add(expression);
			}
			return new ListNestedExpression
			{
				Values = list2
			};
		}

		public FlowtideDotNet.Substrait.Expressions.AggregateFunction VisitAggregateFunction(global::Substrait.Protobuf.AggregateFunction aggregateFunction)
		{
			if (!idToFunctionLookup.TryGetValue(aggregateFunction.FunctionReference, out string value))
			{
				throw new NotImplementedException();
			}
			string extensionUri = value.Substring(0, value.IndexOf(':'));
			string extensionName = value.Substring(value.IndexOf(':') + 1);
			FlowtideDotNet.Substrait.Expressions.AggregateFunction aggregateFunction2 = new FlowtideDotNet.Substrait.Expressions.AggregateFunction
			{
				ExtensionName = extensionName,
				ExtensionUri = extensionUri,
				Arguments = new List<FlowtideDotNet.Substrait.Expressions.Expression>()
			};
			if (aggregateFunction.Args.Count > 0)
			{
				foreach (global::Substrait.Protobuf.Expression arg in aggregateFunction.Args)
				{
					aggregateFunction2.Arguments.Add(VisitExpression(arg));
				}
			}
			else if (aggregateFunction.Arguments.Count > 0)
			{
				foreach (FunctionArgument argument in aggregateFunction.Arguments)
				{
					aggregateFunction2.Arguments.Add(VisitExpression(argument.Value));
				}
			}
			return aggregateFunction2;
		}

		private FlowtideDotNet.Substrait.Expressions.Expression VisitCast(global::Substrait.Protobuf.Expression.Types.Cast cast)
		{
			Span<string> names = Span<string>.Empty;
			return new CastExpression
			{
				Expression = VisitExpression(cast.Input),
				Type = GetType(cast.Type, ref names)
			};
		}

		private SortDirection GetSortDirection(global::Substrait.Protobuf.SortField.Types.SortDirection sortDirection)
		{
			return sortDirection switch
			{
				global::Substrait.Protobuf.SortField.Types.SortDirection.AscNullsFirst => SortDirection.SortDirectionAscNullsFirst, 
				global::Substrait.Protobuf.SortField.Types.SortDirection.DescNullsFirst => SortDirection.SortDirectionDescNullsFirst, 
				global::Substrait.Protobuf.SortField.Types.SortDirection.AscNullsLast => SortDirection.SortDirectionAscNullsLast, 
				global::Substrait.Protobuf.SortField.Types.SortDirection.DescNullsLast => SortDirection.SortDirectionDescNullsLast, 
				global::Substrait.Protobuf.SortField.Types.SortDirection.Clustered => SortDirection.SortDirectionClustered, 
				global::Substrait.Protobuf.SortField.Types.SortDirection.Unspecified => SortDirection.SortDirectionUnspecified, 
				_ => throw new NotImplementedException(sortDirection.ToString()), 
			};
		}

		public FlowtideDotNet.Substrait.Expressions.SortField VisitSortField(global::Substrait.Protobuf.SortField sortField)
		{
			return new FlowtideDotNet.Substrait.Expressions.SortField
			{
				Expression = VisitExpression(sortField.Expr),
				SortDirection = GetSortDirection(sortField.Direction)
			};
		}

		public WindowBound? GetWindowBound(global::Substrait.Protobuf.Expression.Types.WindowFunction.Types.Bound bound)
		{
			return bound.KindCase switch
			{
				global::Substrait.Protobuf.Expression.Types.WindowFunction.Types.Bound.KindOneofCase.CurrentRow => new CurrentRowWindowBound(), 
				global::Substrait.Protobuf.Expression.Types.WindowFunction.Types.Bound.KindOneofCase.Unbounded => new UnboundedWindowBound(), 
				global::Substrait.Protobuf.Expression.Types.WindowFunction.Types.Bound.KindOneofCase.Preceding => new PreceedingRowWindowBound
				{
					Offset = bound.Preceding.Offset
				}, 
				global::Substrait.Protobuf.Expression.Types.WindowFunction.Types.Bound.KindOneofCase.Following => new FollowingRowWindowBound
				{
					Offset = bound.Following.Offset
				}, 
				global::Substrait.Protobuf.Expression.Types.WindowFunction.Types.Bound.KindOneofCase.None => null, 
				_ => throw new NotImplementedException(bound.KindCase.ToString()), 
			};
		}

		public WindowFunction VisitWindowFunction(ConsistentPartitionWindowRel.Types.WindowRelFunction windowRelFunction)
		{
			if (!idToFunctionLookup.TryGetValue(windowRelFunction.FunctionReference, out string value))
			{
				throw new NotImplementedException();
			}
			string extensionUri = value.Substring(0, value.IndexOf(':'));
			string extensionName = value.Substring(value.IndexOf(':') + 1);
			WindowFunction windowFunction = new WindowFunction
			{
				ExtensionName = extensionName,
				ExtensionUri = extensionUri,
				Arguments = new List<FlowtideDotNet.Substrait.Expressions.Expression>(),
				LowerBound = GetWindowBound(windowRelFunction.LowerBound),
				UpperBound = GetWindowBound(windowRelFunction.UpperBound)
			};
			foreach (FunctionArgument argument in windowRelFunction.Arguments)
			{
				windowFunction.Arguments.Add(VisitExpression(argument.Value));
			}
			return windowFunction;
		}

		private FlowtideDotNet.Substrait.Expressions.Expression VisitIfThen(global::Substrait.Protobuf.Expression.Types.IfThen ifThen)
		{
			List<IfClause> list = new List<IfClause>();
			foreach (global::Substrait.Protobuf.Expression.Types.IfThen.Types.IfClause @if in ifThen.Ifs)
			{
				list.Add(new IfClause
				{
					If = VisitExpression(@if.If),
					Then = VisitExpression(@if.Then)
				});
			}
			return new IfThenExpression
			{
				Ifs = list,
				Else = VisitExpression(ifThen.Else)
			};
		}

		private static Literal VisitLiteral(global::Substrait.Protobuf.Expression.Types.Literal literal)
		{
			return literal.LiteralTypeCase switch
			{
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.Boolean => new BoolLiteral
				{
					Value = literal.Boolean
				}, 
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.Null => new NullLiteral(), 
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I32 => new NumericLiteral
				{
					Value = literal.I32
				}, 
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I64 => new NumericLiteral
				{
					Value = literal.I64
				}, 
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.String => new StringLiteral
				{
					Value = literal.String
				}, 
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.FixedChar => new StringLiteral
				{
					Value = literal.FixedChar
				}, 
				global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.Binary => new BinaryLiteral
				{
					Value = literal.Binary.ToByteArray()
				}, 
				_ => throw new NotImplementedException(), 
			};
		}

		public static FieldReference VisitFieldReference(global::Substrait.Protobuf.Expression.Types.FieldReference fieldReference)
		{
			if (fieldReference.ReferenceTypeCase == global::Substrait.Protobuf.Expression.Types.FieldReference.ReferenceTypeOneofCase.DirectReference)
			{
				return VisitDirectReference(fieldReference.DirectReference);
			}
			throw new NotImplementedException();
		}

		private static DirectFieldReference VisitDirectReference(global::Substrait.Protobuf.Expression.Types.ReferenceSegment referenceSegment)
		{
			if (referenceSegment.ReferenceTypeCase == global::Substrait.Protobuf.Expression.Types.ReferenceSegment.ReferenceTypeOneofCase.StructField)
			{
				return VisitStructField(referenceSegment.StructField);
			}
			throw new NotImplementedException();
		}

		private static DirectFieldReference VisitStructField(global::Substrait.Protobuf.Expression.Types.ReferenceSegment.Types.StructField structField)
		{
			return new DirectFieldReference
			{
				ReferenceSegment = new StructReferenceSegment
				{
					Field = structField.Field
				}
			};
		}

		private FlowtideDotNet.Substrait.Expressions.Expression VisitScalarFunction(global::Substrait.Protobuf.Expression.Types.ScalarFunction scalarFunction)
		{
			if (!idToFunctionLookup.TryGetValue(scalarFunction.FunctionReference, out string value))
			{
				throw new NotImplementedException();
			}
			string extensionUri = value.Substring(0, value.IndexOf(':'));
			string extensionName = value.Substring(value.IndexOf(':') + 1);
			List<FlowtideDotNet.Substrait.Expressions.Expression> list = new List<FlowtideDotNet.Substrait.Expressions.Expression>();
			if (scalarFunction.Args != null && scalarFunction.Args.Count > 0)
			{
				foreach (global::Substrait.Protobuf.Expression arg in scalarFunction.Args)
				{
					list.Add(VisitExpression(arg));
				}
			}
			else if (scalarFunction.Arguments != null && scalarFunction.Arguments.Count > 0)
			{
				foreach (FunctionArgument argument in scalarFunction.Arguments)
				{
					list.Add(VisitExpression(argument.Value));
				}
			}
			return new ScalarFunction
			{
				ExtensionUri = extensionUri,
				ExtensionName = extensionName,
				Arguments = list
			};
		}
	}

	private sealed class SubstraitDeserializerImpl
	{
		private readonly global::Substrait.Protobuf.Plan plan;

		private readonly ExpressionDeserializerImpl expressionDeserializer;

		private List<Relation> _relations;

		public SubstraitDeserializerImpl(global::Substrait.Protobuf.Plan plan)
		{
			this.plan = plan;
			expressionDeserializer = new ExpressionDeserializerImpl(plan);
			_relations = new List<Relation>();
		}

		public Plan Convert()
		{
			return VisitPlan(plan);
		}

		private Plan VisitPlan(global::Substrait.Protobuf.Plan plan)
		{
			Plan plan2 = new Plan
			{
				Relations = _relations
			};
			foreach (PlanRel relation in plan.Relations)
			{
				plan2.Relations.Add(VisitPlanRel(relation));
			}
			return plan2;
		}

		private Relation VisitPlanRel(PlanRel planRel)
		{
			return planRel.RelTypeCase switch
			{
				PlanRel.RelTypeOneofCase.Rel => VisitRel(planRel.Rel), 
				PlanRel.RelTypeOneofCase.None => throw new NotImplementedException(), 
				PlanRel.RelTypeOneofCase.Root => VisitRelRoot(planRel.Root), 
				_ => throw new NotImplementedException(), 
			};
		}

		private Relation VisitRelRoot(RelRoot relRoot)
		{
			Relation input = VisitRel(relRoot.Input);
			return new RootRelation
			{
				Input = input,
				Names = ((IEnumerable<string>)relRoot.Names).ToList()
			};
		}

		private Relation VisitRel(Rel rel)
		{
			return rel.RelTypeCase switch
			{
				Rel.RelTypeOneofCase.Project => VisitProject(rel.Project), 
				Rel.RelTypeOneofCase.Read => VisitRead(rel.Read), 
				Rel.RelTypeOneofCase.Filter => VisitFilter(rel.Filter), 
				Rel.RelTypeOneofCase.Join => VisitJoin(rel.Join), 
				Rel.RelTypeOneofCase.Set => VisitSet(rel.Set), 
				Rel.RelTypeOneofCase.Aggregate => VisitAggregate(rel.Aggregate), 
				Rel.RelTypeOneofCase.ExtensionSingle => VisitExtensionSingle(rel.ExtensionSingle), 
				Rel.RelTypeOneofCase.Write => VisitWrite(rel.Write), 
				Rel.RelTypeOneofCase.ExtensionLeaf => VisitExtensionLeaf(rel.ExtensionLeaf), 
				Rel.RelTypeOneofCase.ExtensionMulti => VisitExtensionMulti(rel.ExtensionMulti), 
				Rel.RelTypeOneofCase.Reference => VisitReference(rel.Reference), 
				Rel.RelTypeOneofCase.Window => VisitWindow(rel.Window), 
				Rel.RelTypeOneofCase.MergeJoin => VisitMergeJoin(rel.MergeJoin), 
				Rel.RelTypeOneofCase.Exchange => VisitExchange(rel.Exchange), 
				Rel.RelTypeOneofCase.Fetch => VisitFetch(rel.Fetch), 
				_ => throw new NotImplementedException(rel.RelTypeCase.ToString()), 
			};
		}

		private Relation VisitFetch(FetchRel fetchRel)
		{
			long num = 0L;
			if (fetchRel.OffsetModeCase == FetchRel.OffsetModeOneofCase.Offset)
			{
				num = fetchRel.Offset;
			}
			else if (fetchRel.OffsetModeCase == FetchRel.OffsetModeOneofCase.OffsetExpr)
			{
				if (fetchRel.OffsetExpr.RexTypeCase != global::Substrait.Protobuf.Expression.RexTypeOneofCase.Literal)
				{
					throw new SubstraitParseException("Only literal expressions are supported for fetch offset in Flowtide");
				}
				if (fetchRel.OffsetExpr.Literal.LiteralTypeCase == global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I8)
				{
					num = fetchRel.OffsetExpr.Literal.I8;
				}
				else if (fetchRel.OffsetExpr.Literal.LiteralTypeCase == global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I16)
				{
					num = fetchRel.OffsetExpr.Literal.I16;
				}
				else if (fetchRel.OffsetExpr.Literal.LiteralTypeCase == global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I32)
				{
					num = fetchRel.OffsetExpr.Literal.I32;
				}
				else
				{
					if (fetchRel.OffsetExpr.Literal.LiteralTypeCase != global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I64)
					{
						throw new SubstraitParseException("Only integer literals are supported for fetch offset in Flowtide");
					}
					num = fetchRel.OffsetExpr.Literal.I64;
				}
			}
			long num2 = 0L;
			if (fetchRel.CountModeCase == FetchRel.CountModeOneofCase.Count)
			{
				num2 = fetchRel.Count;
			}
			else if (fetchRel.CountModeCase == FetchRel.CountModeOneofCase.CountExpr)
			{
				if (fetchRel.CountExpr.RexTypeCase != global::Substrait.Protobuf.Expression.RexTypeOneofCase.Literal)
				{
					throw new SubstraitParseException("Only literal expressions are supported for fetch count in Flowtide");
				}
				if (fetchRel.CountExpr.Literal.LiteralTypeCase == global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I8)
				{
					num2 = fetchRel.CountExpr.Literal.I8;
				}
				else if (fetchRel.CountExpr.Literal.LiteralTypeCase == global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I16)
				{
					num2 = fetchRel.CountExpr.Literal.I16;
				}
				else if (fetchRel.CountExpr.Literal.LiteralTypeCase == global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I32)
				{
					num2 = fetchRel.CountExpr.Literal.I32;
				}
				else
				{
					if (fetchRel.CountExpr.Literal.LiteralTypeCase != global::Substrait.Protobuf.Expression.Types.Literal.LiteralTypeOneofCase.I64)
					{
						throw new SubstraitParseException("Only integer literals are supported for fetch count in Flowtide");
					}
					num2 = fetchRel.CountExpr.Literal.I64;
				}
			}
			if (num < int.MinValue || num2 < int.MinValue)
			{
				throw new InvalidOperationException("Offset and count in fetch relation must be greater than int min value");
			}
			if (num2 > int.MaxValue)
			{
				throw new InvalidOperationException("Count in fetch relation cannot be greater than int max value");
			}
			if (num > int.MaxValue)
			{
				throw new InvalidOperationException("Offset in fetch relation cannot be greater than int max value");
			}
			return new FetchRelation
			{
				Input = VisitRel(fetchRel.Input),
				Emit = GetEmit(fetchRel.Common),
				Offset = (int)num,
				Count = (int)num2
			};
		}

		private Relation VisitExchange(ExchangeRel exchange)
		{
			ExchangeKind exchangeKind;
			if (exchange.ExchangeKindCase == ExchangeRel.ExchangeKindOneofCase.ScatterByFields)
			{
				ScatterExchangeKind scatterExchangeKind = new ScatterExchangeKind
				{
					Fields = new List<FieldReference>()
				};
				foreach (global::Substrait.Protobuf.Expression.Types.FieldReference field in exchange.ScatterByFields.Fields)
				{
					scatterExchangeKind.Fields.Add(ExpressionDeserializerImpl.VisitFieldReference(field));
				}
				exchangeKind = scatterExchangeKind;
			}
			else
			{
				if (exchange.ExchangeKindCase != ExchangeRel.ExchangeKindOneofCase.Broadcast)
				{
					throw new NotImplementedException();
				}
				exchangeKind = new BroadcastExchangeKind();
			}
			List<ExchangeTarget> list = new List<ExchangeTarget>();
			foreach (ExchangeRel.Types.ExchangeTarget target in exchange.Targets)
			{
				if (target.Uri == "standard_output")
				{
					list.Add(new StandardOutputExchangeTarget
					{
						PartitionIds = ((IEnumerable<int>)target.PartitionId).ToList()
					});
					continue;
				}
				throw new NotImplementedException(target.Uri ?? "");
			}
			return new ExchangeRelation
			{
				Input = VisitRel(exchange.Input),
				ExchangeKind = exchangeKind,
				Targets = list,
				Emit = GetEmit(exchange.Common),
				PartitionCount = ((exchange.PartitionCount == 0) ? ((int?)null) : new int?(exchange.PartitionCount))
			};
		}

		private JoinType GetJoinType(MergeJoinRel.Types.JoinType joinType)
		{
			return joinType switch
			{
				MergeJoinRel.Types.JoinType.Right => JoinType.Right, 
				MergeJoinRel.Types.JoinType.Outer => JoinType.Outer, 
				MergeJoinRel.Types.JoinType.Inner => JoinType.Inner, 
				MergeJoinRel.Types.JoinType.Left => JoinType.Left, 
				MergeJoinRel.Types.JoinType.Unspecified => JoinType.Unspecified, 
				_ => throw new NotSupportedException(joinType.ToString()), 
			};
		}

		private Relation VisitMergeJoin(MergeJoinRel mergeJoin)
		{
			List<FieldReference> list = new List<FieldReference>();
			List<FieldReference> list2 = new List<FieldReference>();
			foreach (ComparisonJoinKey key in mergeJoin.Keys)
			{
				list.Add(ExpressionDeserializerImpl.VisitFieldReference(key.Left));
				list2.Add(ExpressionDeserializerImpl.VisitFieldReference(key.Right));
			}
			FlowtideDotNet.Substrait.Expressions.Expression postJoinFilter = null;
			if (mergeJoin.PostJoinFilter != null)
			{
				postJoinFilter = expressionDeserializer.VisitExpression(mergeJoin.PostJoinFilter);
			}
			return new MergeJoinRelation
			{
				Emit = GetEmit(mergeJoin.Common),
				Left = VisitRel(mergeJoin.Left),
				Right = VisitRel(mergeJoin.Right),
				LeftKeys = list,
				RightKeys = list2,
				Type = GetJoinType(mergeJoin.Type),
				PostJoinFilter = postJoinFilter
			};
		}

		private Relation VisitWindow(ConsistentPartitionWindowRel windowRel)
		{
			List<FlowtideDotNet.Substrait.Expressions.SortField> list = new List<FlowtideDotNet.Substrait.Expressions.SortField>();
			foreach (global::Substrait.Protobuf.SortField sort in windowRel.Sorts)
			{
				list.Add(expressionDeserializer.VisitSortField(sort));
			}
			List<FlowtideDotNet.Substrait.Expressions.Expression> list2 = new List<FlowtideDotNet.Substrait.Expressions.Expression>();
			foreach (global::Substrait.Protobuf.Expression partitionExpression in windowRel.PartitionExpressions)
			{
				list2.Add(expressionDeserializer.VisitExpression(partitionExpression));
			}
			List<WindowFunction> list3 = new List<WindowFunction>();
			foreach (ConsistentPartitionWindowRel.Types.WindowRelFunction windowFunction in windowRel.WindowFunctions)
			{
				list3.Add(expressionDeserializer.VisitWindowFunction(windowFunction));
			}
			return new ConsistentPartitionWindowRelation
			{
				Input = VisitRel(windowRel.Input),
				Emit = GetEmit(windowRel.Common),
				OrderBy = list,
				PartitionBy = list2,
				WindowFunctions = list3
			};
		}

		private Relation VisitReference(ReferenceRel referenceRel)
		{
			return new ReferenceRelation
			{
				RelationId = referenceRel.SubtreeOrdinal,
				ReferenceOutputLength = _relations[referenceRel.SubtreeOrdinal].OutputLength
			};
		}

		private Relation VisitExtensionMulti(ExtensionMultiRel extensionMulti)
		{
			List<Relation> list = new List<Relation>();
			foreach (Rel input2 in extensionMulti.Inputs)
			{
				list.Add(VisitRel(input2));
			}
			if (Any.GetTypeName(extensionMulti.Detail.TypeUrl) == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.IterationRelation.Descriptor).FullName)
			{
				Relation loopPlan = list[0];
				Relation input = null;
				if (list.Count > 1)
				{
					input = list[1];
				}
				FlowtideDotNet.Substrait.CustomProtobuf.IterationRelation iterationRelation = extensionMulti.Detail.Unpack<FlowtideDotNet.Substrait.CustomProtobuf.IterationRelation>();
				return new FlowtideDotNet.Substrait.Relations.IterationRelation
				{
					Input = input,
					LoopPlan = loopPlan,
					IterationName = iterationRelation.IterationName,
					Emit = GetEmit(extensionMulti.Common)
				};
			}
			throw new NotImplementedException();
		}

		private Relation VisitWrite(WriteRel writeRel)
		{
			Relation input = VisitRel(writeRel.Input);
			FlowtideDotNet.Substrait.Type.NamedStruct tableSchema = expressionDeserializer.ParseNamedStruct(writeRel.TableSchema);
			List<string> list = new List<string>();
			if (writeRel.NamedTable != null)
			{
				list.AddRange((IEnumerable<string>)writeRel.NamedTable.Names);
			}
			FlowtideDotNet.Substrait.Type.NamedTable namedObject = new FlowtideDotNet.Substrait.Type.NamedTable
			{
				Names = list
			};
			List<int> emit = GetEmit(writeRel.Common);
			bool overwrite = false;
			if (writeRel.CreateMode == WriteRel.Types.CreateMode.ReplaceIfExists)
			{
				overwrite = true;
			}
			return new WriteRelation
			{
				Input = input,
				NamedObject = namedObject,
				TableSchema = tableSchema,
				Emit = emit,
				Overwrite = overwrite
			};
		}

		private Relation VisitExtensionLeaf(ExtensionLeafRel extensionLeaf)
		{
			string typeName = Any.GetTypeName(extensionLeaf.Detail.TypeUrl);
			if (typeName == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Descriptor).FullName)
			{
				FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation tableFunctionRelation = extensionLeaf.Detail.Unpack<FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation>();
				FlowtideDotNet.Substrait.Expressions.Expression joinCondition = null;
				if (tableFunctionRelation.JoinCondition != null)
				{
					joinCondition = expressionDeserializer.VisitExpression(tableFunctionRelation.JoinCondition);
				}
				JoinType type = tableFunctionRelation.Type switch
				{
					FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Types.JoinType.Inner => JoinType.Inner, 
					FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Types.JoinType.Left => JoinType.Left, 
					FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Types.JoinType.Unspecified => JoinType.Unspecified, 
					_ => throw new NotImplementedException(), 
				};
				return new FlowtideDotNet.Substrait.Relations.TableFunctionRelation
				{
					TableFunction = expressionDeserializer.VisitTableFunction(tableFunctionRelation.TableFunction),
					Input = null,
					JoinCondition = joinCondition,
					Type = type,
					Emit = GetEmit(extensionLeaf.Common)
				};
			}
			if (typeName == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.IterationReferenceReadRelation.Descriptor).FullName)
			{
				FlowtideDotNet.Substrait.CustomProtobuf.IterationReferenceReadRelation iterationReferenceReadRelation = extensionLeaf.Detail.Unpack<FlowtideDotNet.Substrait.CustomProtobuf.IterationReferenceReadRelation>();
				return new FlowtideDotNet.Substrait.Relations.IterationReferenceReadRelation
				{
					IterationName = iterationReferenceReadRelation.IterationName,
					ReferenceOutputLength = iterationReferenceReadRelation.OutputLength,
					Emit = GetEmit(extensionLeaf.Common)
				};
			}
			if (typeName == ((DescriptorBase)StandardOutputTargetReferenceRelation.Descriptor).FullName)
			{
				StandardOutputTargetReferenceRelation standardOutputTargetReferenceRelation = extensionLeaf.Detail.Unpack<StandardOutputTargetReferenceRelation>();
				return new StandardOutputExchangeReferenceRelation
				{
					RelationId = standardOutputTargetReferenceRelation.RelationId,
					TargetId = standardOutputTargetReferenceRelation.TargetId,
					ReferenceOutputLength = _relations[standardOutputTargetReferenceRelation.RelationId].OutputLength
				};
			}
			throw new NotImplementedException();
		}

		private Relation VisitExtensionSingle(ExtensionSingleRel extensionSingle)
		{
			Relation input = VisitRel(extensionSingle.Input);
			string typeName = Any.GetTypeName(extensionSingle.Detail.TypeUrl);
			if (typeName == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.SubStreamRootRelation.Descriptor).FullName)
			{
				FlowtideDotNet.Substrait.CustomProtobuf.SubStreamRootRelation subStreamRootRelation = extensionSingle.Detail.Unpack<FlowtideDotNet.Substrait.CustomProtobuf.SubStreamRootRelation>();
				return new FlowtideDotNet.Substrait.Relations.SubStreamRootRelation
				{
					Input = input,
					Name = subStreamRootRelation.SubstreamName,
					Emit = GetEmit(extensionSingle.Common)
				};
			}
			if (typeName == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Descriptor).FullName)
			{
				FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation tableFunctionRelation = extensionSingle.Detail.Unpack<FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation>();
				FlowtideDotNet.Substrait.Expressions.Expression joinCondition = expressionDeserializer.VisitExpression(tableFunctionRelation.JoinCondition);
				JoinType type = tableFunctionRelation.Type switch
				{
					FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Types.JoinType.Inner => JoinType.Inner, 
					FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Types.JoinType.Left => JoinType.Left, 
					FlowtideDotNet.Substrait.CustomProtobuf.TableFunctionRelation.Types.JoinType.Unspecified => JoinType.Unspecified, 
					_ => throw new NotImplementedException(), 
				};
				return new FlowtideDotNet.Substrait.Relations.TableFunctionRelation
				{
					TableFunction = expressionDeserializer.VisitTableFunction(tableFunctionRelation.TableFunction),
					Input = input,
					JoinCondition = joinCondition,
					Type = type,
					Emit = GetEmit(extensionSingle.Common)
				};
			}
			if (typeName == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.TopNRelation.Descriptor).FullName)
			{
				FlowtideDotNet.Substrait.CustomProtobuf.TopNRelation topNRelation = extensionSingle.Detail.Unpack<FlowtideDotNet.Substrait.CustomProtobuf.TopNRelation>();
				List<FlowtideDotNet.Substrait.Expressions.SortField> list = new List<FlowtideDotNet.Substrait.Expressions.SortField>();
				foreach (global::Substrait.Protobuf.SortField sort in topNRelation.Sorts)
				{
					list.Add(expressionDeserializer.VisitSortField(sort));
				}
				return new FlowtideDotNet.Substrait.Relations.TopNRelation
				{
					Input = input,
					Count = topNRelation.Count,
					Offset = topNRelation.Offset,
					Emit = GetEmit(extensionSingle.Common),
					Sorts = list
				};
			}
			if (typeName == ((DescriptorBase)FlowtideDotNet.Substrait.CustomProtobuf.BufferRelation.Descriptor).FullName)
			{
				return new FlowtideDotNet.Substrait.Relations.BufferRelation
				{
					Input = input,
					Emit = GetEmit(extensionSingle.Common)
				};
			}
			throw new NotImplementedException();
		}

		private Relation VisitAggregate(AggregateRel aggregateRel)
		{
			AggregateRelation aggregateRelation = new AggregateRelation
			{
				Input = VisitRel(aggregateRel.Input),
				Groupings = new List<AggregateGrouping>(),
				Measures = new List<AggregateMeasure>()
			};
			if (aggregateRel.Groupings.Count > 0)
			{
				foreach (AggregateRel.Types.Grouping grouping in aggregateRel.Groupings)
				{
					AggregateGrouping aggregateGrouping = new AggregateGrouping
					{
						GroupingExpressions = new List<FlowtideDotNet.Substrait.Expressions.Expression>()
					};
					if (grouping.ExpressionReferences.Count > 0)
					{
						foreach (uint expressionReference in grouping.ExpressionReferences)
						{
							aggregateGrouping.GroupingExpressions.Add(expressionDeserializer.VisitExpression(aggregateRel.GroupingExpressions[(int)expressionReference]));
						}
					}
					else
					{
						foreach (global::Substrait.Protobuf.Expression groupingExpression in grouping.GroupingExpressions)
						{
							aggregateGrouping.GroupingExpressions.Add(expressionDeserializer.VisitExpression(groupingExpression));
						}
					}
					aggregateRelation.Groupings.Add(aggregateGrouping);
				}
			}
			if (aggregateRel.Measures.Count > 0)
			{
				foreach (AggregateRel.Types.Measure measure2 in aggregateRel.Measures)
				{
					FlowtideDotNet.Substrait.Expressions.Expression filter = null;
					if (measure2.Filter != null)
					{
						filter = expressionDeserializer.VisitExpression(measure2.Filter);
					}
					FlowtideDotNet.Substrait.Expressions.AggregateFunction measure = expressionDeserializer.VisitAggregateFunction(measure2.Measure_);
					aggregateRelation.Measures.Add(new AggregateMeasure
					{
						Filter = filter,
						Measure = measure
					});
				}
			}
			return aggregateRelation;
		}

		private static List<int>? GetEmit(RelCommon relCommon)
		{
			if (relCommon == null)
			{
				return null;
			}
			return relCommon.EmitKindCase switch
			{
				RelCommon.EmitKindOneofCase.Direct => null, 
				RelCommon.EmitKindOneofCase.Emit => ((IEnumerable<int>)relCommon.Emit.OutputMapping).ToList(), 
				_ => throw new NotImplementedException(), 
			};
		}

		private Relation VisitSet(SetRel setRel)
		{
			SetRelation setRelation = new SetRelation
			{
				Inputs = new List<Relation>()
			};
			for (int i = 0; i < setRel.Inputs.Count; i++)
			{
				setRelation.Inputs.Add(VisitRel(setRel.Inputs[i]));
			}
			setRelation.Operation = SetOperation.UnionAll;
			setRelation.Emit = GetEmit(setRel.Common);
			return setRelation;
		}

		private Relation VisitProject(ProjectRel projectRel)
		{
			Relation input = VisitRel(projectRel.Input);
			ProjectRelation projectRelation = new ProjectRelation
			{
				Input = input,
				Expressions = new List<FlowtideDotNet.Substrait.Expressions.Expression>(),
				Emit = GetEmit(projectRel.Common)
			};
			if (projectRel.Expressions.Count > 0)
			{
				foreach (global::Substrait.Protobuf.Expression expression in projectRel.Expressions)
				{
					projectRelation.Expressions.Add(expressionDeserializer.VisitExpression(expression));
				}
			}
			return projectRelation;
		}

		private Relation VisitRead(ReadRel readRel)
		{
			List<string> list = new List<string>();
			list.AddRange((IEnumerable<string>)readRel.BaseSchema.Names);
			Struct obj = null;
			if (readRel.BaseSchema.Struct != null)
			{
				obj = expressionDeserializer.ParseStruct(readRel.BaseSchema.Struct);
			}
			FlowtideDotNet.Substrait.Type.NamedStruct baseSchema = new FlowtideDotNet.Substrait.Type.NamedStruct
			{
				Names = list,
				Struct = obj
			};
			if (readRel.ReadTypeCase == ReadRel.ReadTypeOneofCase.NamedTable)
			{
				List<string> list2 = new List<string>();
				list2.AddRange((IEnumerable<string>)readRel.NamedTable.Names);
				return new ReadRelation
				{
					BaseSchema = baseSchema,
					NamedTable = new FlowtideDotNet.Substrait.Type.NamedTable
					{
						Names = list2
					},
					Emit = GetEmit(readRel.Common)
				};
			}
			if (readRel.ReadTypeCase == ReadRel.ReadTypeOneofCase.VirtualTable)
			{
				List<StructExpression> list3 = new List<StructExpression>();
				foreach (global::Substrait.Protobuf.Expression.Types.Nested.Types.Struct expression in readRel.VirtualTable.Expressions)
				{
					list3.Add(expressionDeserializer.VisitStruct(expression));
				}
				VirtualTable values = new VirtualTable
				{
					Expressions = list3
				};
				return new VirtualTableReadRelation
				{
					BaseSchema = baseSchema,
					Emit = GetEmit(readRel.Common),
					Values = values
				};
			}
			throw new NotImplementedException("Read relation must have either named table or virtual table expressions");
		}

		private Relation VisitFilter(FilterRel filterRel)
		{
			Relation input = VisitRel(filterRel.Input);
			if (filterRel.Condition == null)
			{
				throw new InvalidOperationException("Filter must have a condition");
			}
			return new FilterRelation
			{
				Input = input,
				Condition = expressionDeserializer.VisitExpression(filterRel.Condition),
				Emit = GetEmit(filterRel.Common)
			};
		}

		private Relation VisitJoin(JoinRel joinRel)
		{
			Relation left = VisitRel(joinRel.Left);
			Relation right = VisitRel(joinRel.Right);
			JoinType type = joinRel.Type switch
			{
				JoinRel.Types.JoinType.Right => JoinType.Right, 
				JoinRel.Types.JoinType.Inner => JoinType.Inner, 
				JoinRel.Types.JoinType.Left => JoinType.Left, 
				_ => throw new NotSupportedException("Join type not supported"), 
			};
			JoinRelation joinRelation = new JoinRelation
			{
				Left = left,
				Right = right,
				Emit = GetEmit(joinRel.Common),
				Type = type
			};
			if (joinRel.Expression != null)
			{
				joinRelation.Expression = expressionDeserializer.VisitExpression(joinRel.Expression);
			}
			if (joinRel.PostJoinFilter != null)
			{
				joinRelation.PostJoinFilter = expressionDeserializer.VisitExpression(joinRel.PostJoinFilter);
			}
			return joinRelation;
		}
	}

	public Plan Deserialize(string json)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		TypeRegistry val = TypeRegistry.FromMessages((MessageDescriptor[])(object)new MessageDescriptor[2]
		{
			FlowtideDotNet.Substrait.CustomProtobuf.IterationReferenceReadRelation.Descriptor,
			FlowtideDotNet.Substrait.CustomProtobuf.IterationRelation.Descriptor
		});
		global::Substrait.Protobuf.Plan plan = new JsonParser(new Settings(300, val)).Parse<global::Substrait.Protobuf.Plan>(json);
		return Deserialize(plan);
	}

	public static Plan DeserializeFromJson(string json)
	{
		return new SubstraitDeserializer().Deserialize(json);
	}

	public Plan Deserialize(global::Substrait.Protobuf.Plan plan)
	{
		return new SubstraitDeserializerImpl(plan).Convert();
	}
}
