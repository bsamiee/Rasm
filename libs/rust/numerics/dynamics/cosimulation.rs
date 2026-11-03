use core::{convert::identity, hash::Hash, iter, ops::ControlFlow};
use std::{collections::HashMap, path::Path};

use fmi::{
    fmi2::{
        self, Fmi2Error, Fmi2Res,
        import::Fmi2Import,
        instance::{CoSimulation as _, Common as _},
        schema::{ScalarVariableElement, SimpleTypeElement},
    },
    fmi3::{
        self, CoSimulation as _, Fmi3Error, Fmi3Model as _, GetSet as _,
        import::Fmi3Import,
        schema::{ArrayableVariableTrait, FloatVariableAlias, TypeDefinition, TypedVariableTrait as _, Variable, VariableAlias},
    },
    import::{from_path, peek_descr_path},
    schema::traits::{DefaultExperiment, FmiModelDescription as _},
    traits::{FmiImport as _, FmiInstance, FmiStatus},
};
use itertools::Itertools as _;
use uom::{
    fmt::DisplayStyle::Abbreviation,
    si::{f64::Time, time::second},
};

use crate::{BaseUnit, Causality, DynamicsError, Grid, Kind, Operation, Port};

// --- [TYPES] ---------------------------------------------------------------------------

#[derive(Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
enum Fmi2Type {
    Real,
    Integer,
    Boolean,
}

#[derive(Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
enum Fmi3Type {
    Float32,
    Float64,
    Int8,
    UInt8,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Boolean,
}

// --- [SERVICES] ------------------------------------------------------------------------

enum Instance {
    Fmi2 { instance: Box<fmi2::instance::InstanceCS>, import: Fmi2Import, variables: Vec<(Fmi2Type, u32)> },
    Fmi3 { instance: Box<fmi3::instance::InstanceCS>, import: Fmi3Import, variables: Vec<(Fmi3Type, u32)> },
}

#[derive(derive_more::Debug, derive_more::AsRef)]
pub struct Fmu {
    #[debug(skip)]
    instance: Instance,
    tolerance: Option<f64>,
    #[as_ref([Port])]
    ports: Vec<Port>,
}

impl Fmu {
    #[must_use]
    pub fn default_experiment(&self) -> &dyn DefaultExperiment {
        match &self.instance {
            Instance::Fmi2 { instance: _, import, variables: _ } => import.model_description(),
            Instance::Fmi3 { instance: _, import, variables: _ } => import.model_description(),
        }
    }

    pub(crate) fn set(&mut self, references: &[usize], values: &[f64]) -> Result<(), DynamicsError> {
        fn stored<T, R, E>(value_references: &[u32], written: &[f64], narrow: impl Fn(f64) -> T, setter: impl FnOnce(&[u32], &[T]) -> Result<R, E>) -> Result<(), DynamicsError>
        where E: Into<fmi::Error> {
            setter(value_references, &written.iter().copied().map(narrow).collect_vec()).map(drop).map_err(failure(Operation::Set))
        }
        let members = iter::zip(references.iter().copied(), values.iter().copied());
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _, variables } => grouped(variables, members)?.try_for_each(|(typed, held, written)| match typed {
                Fmi2Type::Real => stored(&held, &written, identity, |vrs, narrowed| instance.set_real(vrs, narrowed)),
                Fmi2Type::Integer => stored(&held, &written, |value| value.round().to_int_saturating::<i32>(), |vrs, narrowed| instance.set_integer(vrs, narrowed)),
                Fmi2Type::Boolean => stored(&held, &written, |value| i32::from(value != 0.0), |vrs, narrowed| instance.set_boolean(vrs, narrowed)),
            }),
            Instance::Fmi3 { instance, import: _, variables } => grouped(variables, members)?.try_for_each(|(typed, held, written)| match typed {
                Fmi3Type::Float32 => stored(&held, &written, f64::cast::<f32>, |vrs, narrowed| instance.set_float32(vrs, narrowed)),
                Fmi3Type::Float64 => stored(&held, &written, identity, |vrs, narrowed| instance.set_float64(vrs, narrowed)),
                Fmi3Type::Int8 => stored(&held, &written, |value| value.round().to_int_saturating::<i8>(), |vrs, narrowed| instance.set_int8(vrs, narrowed)),
                Fmi3Type::UInt8 => stored(&held, &written, |value| value.round().to_int_saturating::<u8>(), |vrs, narrowed| instance.set_uint8(vrs, narrowed)),
                Fmi3Type::Int16 => stored(&held, &written, |value| value.round().to_int_saturating::<i16>(), |vrs, narrowed| instance.set_int16(vrs, narrowed)),
                Fmi3Type::UInt16 => stored(&held, &written, |value| value.round().to_int_saturating::<u16>(), |vrs, narrowed| instance.set_uint16(vrs, narrowed)),
                Fmi3Type::Int32 => stored(&held, &written, |value| value.round().to_int_saturating::<i32>(), |vrs, narrowed| instance.set_int32(vrs, narrowed)),
                Fmi3Type::UInt32 => stored(&held, &written, |value| value.round().to_int_saturating::<u32>(), |vrs, narrowed| instance.set_uint32(vrs, narrowed)),
                Fmi3Type::Boolean => stored(&held, &written, |value| value != 0.0, |vrs, narrowed| instance.set_boolean(vrs, narrowed)),
            }),
        }
    }

    pub(crate) fn get(&mut self, references: &[usize]) -> Result<Vec<f64>, DynamicsError> {
        fn fetched<T, R, E>(value_references: &[u32], widen: impl Fn(T) -> f64, getter: impl FnOnce(&[u32], &mut [T]) -> Result<R, E>) -> Result<Vec<f64>, DynamicsError>
        where
            T: Clone + Default,
            E: Into<fmi::Error>,
        {
            let mut values = vec![T::default(); value_references.len()];
            getter(value_references, &mut values).map_err(failure(Operation::Get))?;
            Ok(values.into_iter().map(widen).collect())
        }
        fn gathered<S>(variables: &[(S, u32)], references: &[usize], mut fetch: impl FnMut(S, &[u32]) -> Result<Vec<f64>, DynamicsError>) -> Result<Vec<f64>, DynamicsError>
        where S: Copy + Ord + Hash {
            grouped(variables, iter::zip(references.iter().copied(), 0_usize..))?
                .map(|(typed, held, positions)| fetch(typed, &held).map(|values| iter::zip(positions, values)))
                .flatten_ok()
                .process_results(|located| located.sorted_unstable_by_key(|&(position, _)| position).map(|(_, value)| value).collect())
        }
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _, variables } => gathered(variables, references, |typed, held| match typed {
                Fmi2Type::Real => fetched(held, identity, |vrs, values| instance.get_real(vrs, values)),
                Fmi2Type::Integer => fetched(held, f64::from, |vrs, values: &mut [i32]| instance.get_integer(vrs, values)),
                Fmi2Type::Boolean => fetched(held, f64::from, |vrs, values: &mut [i32]| instance.get_boolean(vrs, values)),
            }),
            Instance::Fmi3 { instance, import: _, variables } => gathered(variables, references, |typed, held| match typed {
                Fmi3Type::Float32 => fetched(held, f64::from, |vrs, values: &mut [f32]| instance.get_float32(vrs, values)),
                Fmi3Type::Float64 => fetched(held, identity, |vrs, values| instance.get_float64(vrs, values)),
                Fmi3Type::Int8 => fetched(held, f64::from, |vrs, values: &mut [i8]| instance.get_int8(vrs, values)),
                Fmi3Type::UInt8 => fetched(held, f64::from, |vrs, values: &mut [u8]| instance.get_uint8(vrs, values)),
                Fmi3Type::Int16 => fetched(held, f64::from, |vrs, values: &mut [i16]| instance.get_int16(vrs, values)),
                Fmi3Type::UInt16 => fetched(held, f64::from, |vrs, values: &mut [u16]| instance.get_uint16(vrs, values)),
                Fmi3Type::Int32 => fetched(held, f64::from, |vrs, values: &mut [i32]| instance.get_int32(vrs, values)),
                Fmi3Type::UInt32 => fetched(held, f64::from, |vrs, values: &mut [u32]| instance.get_uint32(vrs, values)),
                Fmi3Type::Boolean => fetched(held, f64::from, |vrs, values: &mut [bool]| instance.get_boolean(vrs, values)),
            }),
        }
    }

    pub(crate) fn initialize(&mut self, grid: &Grid) -> Result<(), DynamicsError> {
        let variable = match &self.instance {
            Instance::Fmi2 { instance: _, import, variables: _ } =>
                import.model_description().co_simulation.as_ref().and_then(|co_simulation| co_simulation.can_handle_variable_communication_step_size),
            Instance::Fmi3 { instance: _, import, variables: _ } =>
                import.model_description().co_simulation.as_ref().and_then(|co_simulation| co_simulation.can_handle_variable_communication_step_size),
        };
        let (start, stop) = (grid.first().get::<second>(), grid.last().get::<second>());
        let resolution = Time::new::<second>(2.0 * (stop.abs().next_up() - stop.abs()));
        if let Some((shortest, longest)) = grid.as_ref().array_windows().map(|&[time, next]| next - time).minmax().into_option()
            && !variable.unwrap_or_default()
            && longest - shortest > resolution
        {
            return Err(failure(Operation::Initialize)(fmi::Error::UnsupportedInterface(format!(
                "variable communication step size from {} to {}",
                shortest.into_format_args(second, Abbreviation),
                longest.into_format_args(second, Abbreviation)
            ))));
        }
        let (stop, tolerance) = (Some(stop), self.tolerance.or_else(|| self.default_experiment().tolerance()));
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _, variables: _ } => FmiInstance::enter_initialization_mode(&mut **instance, tolerance, start, stop).map(drop).map_err(failure(Operation::Initialize)),
            Instance::Fmi3 { instance, import: _, variables: _ } => FmiInstance::enter_initialization_mode(&mut **instance, tolerance, start, stop).map(drop).map_err(failure(Operation::Initialize)),
        }
    }

    pub(crate) fn exit(&mut self) -> Result<(), DynamicsError> {
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _, variables: _ } => FmiInstance::exit_initialization_mode(&mut **instance).map(drop).map_err(failure(Operation::Initialize)),
            Instance::Fmi3 { instance, import: _, variables: _ } => FmiInstance::exit_initialization_mode(&mut **instance).map(drop).map_err(failure(Operation::Initialize)),
        }
    }

    pub(crate) fn step(&mut self, time: Time, step: Time) -> Result<ControlFlow<Time>, DynamicsError> {
        let (time, step) = (time.get::<second>(), step.get::<second>());
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _, variables: _ } => {
                let mut settled = instance.do_step(time, step, true);
                while matches!(settled, Ok(Fmi2Res::Pending)) {
                    settled = instance.do_step_status().and_then(FmiStatus::ok);
                }
                match settled {
                    Ok(Fmi2Res::OK | Fmi2Res::Warning | Fmi2Res::Pending) => Ok(ControlFlow::Continue(())),
                    Err(Fmi2Error::Discard) => instance.terminated().and_then(|terminated| {
                        terminated.ok_or(Fmi2Error::Discard)?;
                        instance.last_successful_time().map(|last_successful_time| ControlFlow::Break(Time::new::<second>(last_successful_time)))
                    }),
                    Err(source @ (Fmi2Error::TypesPlatformMismatch(_) | Fmi2Error::Error | Fmi2Error::Fatal)) => Err(source),
                }
                .map_err(failure(Operation::Step))
            }
            Instance::Fmi3 { instance, import: _, variables: _ } => {
                let (mut event_handling_needed, mut terminate_simulation, mut early_return, mut last_successful_time) = (false, false, false, time);
                let stepped = instance.do_step(time, step, true, &mut event_handling_needed, &mut terminate_simulation, &mut early_return, &mut last_successful_time);
                match (stepped, terminate_simulation) {
                    (Ok(_) | Err(Fmi3Error::Discard), true) => Ok(ControlFlow::Break(Time::new::<second>(last_successful_time))),
                    (Ok(_), false) => Ok(ControlFlow::Continue(())),
                    (Err(source), _) => Err(failure(Operation::Step)(source)),
                }
            }
        }
    }

    pub(crate) fn terminate(&mut self) -> Result<(), DynamicsError> {
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _, variables: _ } => FmiInstance::terminate(&mut **instance).map(drop).map_err(failure(Operation::Terminate)),
            Instance::Fmi3 { instance, import: _, variables: _ } => FmiInstance::terminate(&mut **instance).map(drop).map_err(failure(Operation::Terminate)),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn fmu(path: impl AsRef<Path>, tolerance: Option<f64>) -> Result<Fmu, DynamicsError> {
    fn numbered<S>(members: impl Iterator<Item = ((S, u32), String, Causality, Kind)>) -> (Vec<(S, u32)>, Vec<Port>) {
        members.enumerate().map(|(reference, (variable, name, causality, kind))| (variable, Port { name, causality, reference, kind })).unzip()
    }
    let path = path.as_ref();
    let base_unit = |exponents: [Option<i32>; 8], factor: Option<f64>, offset: Option<f64>, relative_quantity: Option<bool>| BaseUnit {
        exponents: exponents.map(Option::unwrap_or_default),
        factor: factor.unwrap_or(1.0),
        offset: offset.filter(|_| !relative_quantity.unwrap_or_default()).unwrap_or_default(),
    };
    match peek_descr_path(path).and_then(|description| description.version().map_err(fmi::Error::from)).map_err(failure(Operation::Import(path.to_path_buf())))?.major {
        2 => {
            let import = from_path::<Fmi2Import>(path).map_err(failure(Operation::Import(path.to_path_buf())))?;
            let description = import.model_description();
            let bases =
                description.unit_definitions.iter().flat_map(|definitions| &definitions.units).filter_map(|unit| Some((unit.name.as_str(), unit.base_unit.as_ref()?))).collect::<HashMap<_, _>>();
            let units = description
                .type_definitions
                .iter()
                .flat_map(|definitions| &definitions.types)
                .filter_map(|definition| {
                    let SimpleTypeElement::Real(attributes) = &definition.elem else { return None };
                    let base = bases.get(attributes.unit.as_deref()?)?;
                    Some((definition.name.as_str(), base_unit([base.kg, base.m, base.s, base.a, base.k, base.mol, base.cd, base.rad], base.factor, base.offset, attributes.relative_quantity)))
                })
                .collect::<HashMap<_, _>>();
            let (variables, ports) = numbered(description.model_variables.variables.iter().filter_map(|variable| {
                let (typed, kind) = match &variable.elem {
                    ScalarVariableElement::Real(real) => (Fmi2Type::Real, Kind::Real(real.declared_type.as_deref().and_then(|declared| units.get(declared).copied()))),
                    ScalarVariableElement::Integer(_) | ScalarVariableElement::Enumeration => (Fmi2Type::Integer, Kind::Integer),
                    ScalarVariableElement::Boolean(_) => (Fmi2Type::Boolean, Kind::Boolean),
                    ScalarVariableElement::String => return None,
                };
                let causality = match variable.causality {
                    fmi2::schema::Causality::Parameter => Some(Causality::Parameter),
                    fmi2::schema::Causality::Input => Some(Causality::Input),
                    fmi2::schema::Causality::Output => Some(Causality::Output),
                    fmi2::schema::Causality::CalculatedParameter | fmi2::schema::Causality::Local | fmi2::schema::Causality::Independent => None,
                }?;
                Some(((typed, variable.value_reference), variable.name.clone(), causality, kind))
            }));
            let instance = import.instantiate_cs(&description.model_name, false, false).map_err(failure(Operation::Instantiate(path.to_path_buf())))?;
            Ok(Fmu { instance: Instance::Fmi2 { instance: Box::new(instance), import, variables }, tolerance, ports })
        }
        3 => {
            let import = from_path::<Fmi3Import>(path).map_err(failure(Operation::Import(path.to_path_buf())))?;
            let description = import.model_description();
            let bases =
                description.unit_definitions.iter().flat_map(|definitions| &definitions.units).filter_map(|unit| Some((unit.name.as_str(), unit.base_unit.as_ref()?))).collect::<HashMap<_, _>>();
            let units = description
                .type_definitions
                .iter()
                .flat_map(|definitions| &definitions.type_definitions)
                .filter_map(|definition| {
                    let (name, unit, relative_quantity) = match definition {
                        TypeDefinition::Float32(attributes) => (&attributes.name, &attributes.unit, attributes.relative_quantity),
                        TypeDefinition::Float64(attributes) => (&attributes.name, &attributes.unit, attributes.relative_quantity),
                        TypeDefinition::Int8(_)
                        | TypeDefinition::UInt8(_)
                        | TypeDefinition::Int16(_)
                        | TypeDefinition::UInt16(_)
                        | TypeDefinition::Int32(_)
                        | TypeDefinition::UInt32(_)
                        | TypeDefinition::Int64(_)
                        | TypeDefinition::UInt64(_)
                        | TypeDefinition::Boolean(_)
                        | TypeDefinition::String(_)
                        | TypeDefinition::Binary(_)
                        | TypeDefinition::Enumeration(_)
                        | TypeDefinition::Clock(_) => return None,
                    };
                    let base = bases.get(unit.as_deref()?)?;
                    Some((name.as_str(), base_unit([base.kg, base.m, base.s, base.a, base.k, base.mol, base.cd, base.rad], base.factor, base.offset, relative_quantity)))
                })
                .collect::<HashMap<_, _>>();
            let real = |declared: Option<&str>| Kind::Real(declared.and_then(|name| units.get(name).copied()));
            let float_aliases = |aliases: &[FloatVariableAlias]| aliases.iter().map(|alias| alias.name.clone()).collect_vec();
            let plain_aliases = |aliases: &[VariableAlias]| aliases.iter().map(|alias| alias.name.clone()).collect_vec();
            let (variables, ports) = numbered(
                description
                    .model_variables
                    .variables
                    .iter()
                    .filter_map(|variable| {
                        let (scalar, typed, kind, aliases): (&dyn ArrayableVariableTrait, _, _, _) = match variable {
                            Variable::Float32(float) => (float, Fmi3Type::Float32, real(float.declared_type()), float_aliases(&float.aliases)),
                            Variable::Float64(float) => (float, Fmi3Type::Float64, real(float.declared_type()), float_aliases(&float.aliases)),
                            Variable::Int8(integer) => (integer, Fmi3Type::Int8, Kind::Integer, plain_aliases(&integer.aliases)),
                            Variable::UInt8(integer) => (integer, Fmi3Type::UInt8, Kind::Integer, plain_aliases(&integer.aliases)),
                            Variable::Int16(integer) => (integer, Fmi3Type::Int16, Kind::Integer, plain_aliases(&integer.aliases)),
                            Variable::UInt16(integer) => (integer, Fmi3Type::UInt16, Kind::Integer, plain_aliases(&integer.aliases)),
                            Variable::Int32(integer) => (integer, Fmi3Type::Int32, Kind::Integer, plain_aliases(&integer.aliases)),
                            Variable::UInt32(integer) => (integer, Fmi3Type::UInt32, Kind::Integer, plain_aliases(&integer.aliases)),
                            Variable::Boolean(boolean) => (boolean, Fmi3Type::Boolean, Kind::Boolean, plain_aliases(&boolean.aliases)),
                            Variable::Int64(_) | Variable::UInt64(_) | Variable::String(_) | Variable::Binary(_) | Variable::Clock(_) => return None,
                        };
                        let causality = match scalar.causality() {
                            fmi3::schema::Causality::Parameter => Some(Causality::Parameter),
                            fmi3::schema::Causality::Input => Some(Causality::Input),
                            fmi3::schema::Causality::Output => Some(Causality::Output),
                            fmi3::schema::Causality::CalculatedParameter
                            | fmi3::schema::Causality::StructuralParameter
                            | fmi3::schema::Causality::Local
                            | fmi3::schema::Causality::Independent
                            | fmi3::schema::Causality::Dependent => None,
                        }
                        .filter(|_| scalar.dimensions().is_empty())?;
                        let reference = scalar.value_reference();
                        Some(iter::once(scalar.name().to_owned()).chain(aliases).map(move |name| ((typed, reference), name, causality, kind)))
                    })
                    .flatten(),
            );
            let instance = import.instantiate_cs(&description.model_name, false, false, false, false, &[]).map_err(failure(Operation::Instantiate(path.to_path_buf())))?;
            Ok(Fmu { instance: Instance::Fmi3 { instance: Box::new(instance), import, variables }, tolerance, ports })
        }
        major => Err(DynamicsError::Version { path: path.to_path_buf(), major }),
    }
}

fn grouped<S, M>(variables: &[(S, u32)], members: impl IntoIterator<Item = (usize, M)>) -> Result<impl Iterator<Item = (S, Vec<u32>, Vec<M>)>, DynamicsError>
where S: Copy + Ord + Hash {
    members
        .into_iter()
        .map(|(reference, member)| variables.get(reference).map(|&(typed, value_reference)| (typed, (value_reference, member))).ok_or(DynamicsError::Reference { reference }))
        .process_results(|located| {
            located.into_group_map().into_iter().sorted_unstable_by_key(|&(typed, _)| typed).map(|(typed, group)| {
                let (held, carried) = group.into_iter().unzip();
                (typed, held, carried)
            })
        })
}

const fn failure<E>(operation: Operation) -> impl FnOnce(E) -> DynamicsError
where E: Into<fmi::Error> {
    move |source| DynamicsError::Fmi { operation, source: source.into() }
}
