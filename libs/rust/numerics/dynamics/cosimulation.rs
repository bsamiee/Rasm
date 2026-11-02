use core::ops::ControlFlow;
use std::{collections::HashMap, path::Path};

use fmi::{
    fmi2::{
        self, Fmi2Error,
        import::Fmi2Import,
        instance::{CoSimulation as _, Common as _},
        schema::{ScalarVariableElement, SimpleTypeElement},
    },
    fmi3::{
        self, CoSimulation as _, Fmi3Error, Fmi3Model as _, GetSet as _,
        import::Fmi3Import,
        schema::{AbstractVariableTrait as _, ArrayableVariableTrait as _, TypeDefinition, TypedVariableTrait as _},
    },
    import::{from_path, peek_descr_path},
    schema::traits::{DefaultExperiment as _, FmiModelDescription as _},
    traits::{FmiImport as _, FmiInstance},
};
use uom::si::{f64::Time, time::second};

use crate::{BaseUnit, Causality, DynamicsError, Operation, Port};

// --- [SERVICES] ------------------------------------------------------------------------

enum Instance {
    Fmi2 { instance: Box<fmi2::instance::InstanceCS>, import: Fmi2Import },
    Fmi3 { instance: Box<fmi3::instance::InstanceCS>, import: Fmi3Import },
}

#[derive(derive_more::Debug, derive_more::AsRef)]
pub struct Fmu {
    #[debug(skip)]
    instance: Instance,
    #[as_ref([Port])]
    ports: Vec<Port>,
}

impl Fmu {
    pub(crate) fn set(&mut self, references: &[u32], values: &[f64]) -> Result<(), DynamicsError> {
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _ } => instance.set_real(references, values).map(drop).map_err(failure(Operation::Set)),
            Instance::Fmi3 { instance, import: _ } => instance.set_float64(references, values).map(drop).map_err(failure(Operation::Set)),
        }
    }

    pub(crate) fn get(&mut self, references: &[u32]) -> Result<Vec<f64>, DynamicsError> {
        let mut values = vec![0.0; references.len()];
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _ } => instance.get_real(references, &mut values).map(|_| values).map_err(failure(Operation::Get)),
            Instance::Fmi3 { instance, import: _ } => instance.get_float64(references, &mut values).map(|_| values).map_err(failure(Operation::Get)),
        }
    }

    pub(crate) fn initialize(&mut self, start: Time, stop: Time) -> Result<(), DynamicsError> {
        let (start, stop) = (start.get::<second>(), Some(stop.get::<second>()));
        match &mut self.instance {
            Instance::Fmi2 { instance, import } =>
                FmiInstance::enter_initialization_mode(&mut **instance, import.model_description().tolerance(), start, stop).map(drop).map_err(failure(Operation::Initialize)),
            Instance::Fmi3 { instance, import } =>
                FmiInstance::enter_initialization_mode(&mut **instance, import.model_description().tolerance(), start, stop).map(drop).map_err(failure(Operation::Initialize)),
        }
    }

    pub(crate) fn exit(&mut self) -> Result<(), DynamicsError> {
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _ } => FmiInstance::exit_initialization_mode(&mut **instance).map(drop).map_err(failure(Operation::Initialize)),
            Instance::Fmi3 { instance, import: _ } => FmiInstance::exit_initialization_mode(&mut **instance).map(drop).map_err(failure(Operation::Initialize)),
        }
    }

    pub(crate) fn step(&mut self, time: Time, step: Time) -> Result<ControlFlow<()>, DynamicsError> {
        let (time, step) = (time.get::<second>(), step.get::<second>());
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _ } => match instance.do_step(time, step, true) {
                Ok(_) => Ok(ControlFlow::Continue(())),
                Err(Fmi2Error::Discard) => instance.terminated().and_then(|terminated| terminated.then_some(ControlFlow::Break(())).ok_or(Fmi2Error::Discard)),
                Err(source @ (Fmi2Error::TypesPlatformMismatch(_) | Fmi2Error::Error | Fmi2Error::Fatal)) => Err(source),
            }
            .map_err(failure(Operation::Step)),
            Instance::Fmi3 { instance, import: _ } => {
                let (mut event_handling_needed, mut terminate_simulation, mut early_return, mut last_successful_time) = (false, false, false, time);
                let stepped = instance.do_step(time, step, true, &mut event_handling_needed, &mut terminate_simulation, &mut early_return, &mut last_successful_time);
                match (stepped, terminate_simulation) {
                    (Ok(_) | Err(Fmi3Error::Discard), true) => Ok(ControlFlow::Break(())),
                    (Ok(_), false) => Ok(ControlFlow::Continue(())),
                    (Err(source), _) => Err(failure(Operation::Step)(source)),
                }
            }
        }
    }

    pub(crate) fn terminate(&mut self) -> Result<(), DynamicsError> {
        match &mut self.instance {
            Instance::Fmi2 { instance, import: _ } => FmiInstance::terminate(&mut **instance).map(drop).map_err(failure(Operation::Terminate)),
            Instance::Fmi3 { instance, import: _ } => FmiInstance::terminate(&mut **instance).map(drop).map_err(failure(Operation::Terminate)),
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

pub fn fmu(path: impl AsRef<Path>) -> Result<Fmu, DynamicsError> {
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
            let ports = description
                .model_variables
                .variables
                .iter()
                .filter_map(|variable| {
                    let ScalarVariableElement::Real(real) = &variable.elem else { return None };
                    let causality = match variable.causality {
                        fmi2::schema::Causality::Parameter => Some(Causality::Parameter),
                        fmi2::schema::Causality::Input => Some(Causality::Input),
                        fmi2::schema::Causality::Output => Some(Causality::Output),
                        fmi2::schema::Causality::CalculatedParameter | fmi2::schema::Causality::Local | fmi2::schema::Causality::Independent => None,
                    }?;
                    Some(Port { name: variable.name.clone(), causality, reference: variable.value_reference, unit: real.declared_type.as_deref().and_then(|declared| units.get(declared).copied()) })
                })
                .collect();
            let instance = import.instantiate_cs(&description.model_name, false, false).map_err(failure(Operation::Instantiate(path.to_path_buf())))?;
            Ok(Fmu { instance: Instance::Fmi2 { instance: Box::new(instance), import }, ports })
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
                    let TypeDefinition::Float64(attributes) = definition else { return None };
                    let base = bases.get(attributes.unit.as_deref()?)?;
                    Some((attributes.name.as_str(), base_unit([base.kg, base.m, base.s, base.a, base.k, base.mol, base.cd, base.rad], base.factor, base.offset, attributes.relative_quantity)))
                })
                .collect::<HashMap<_, _>>();
            let ports = description
                .model_variables
                .float64()
                .into_iter()
                .filter(|variable| variable.dimensions().is_empty())
                .filter_map(|variable| {
                    let causality = match variable.causality() {
                        fmi3::schema::Causality::Parameter => Some(Causality::Parameter),
                        fmi3::schema::Causality::Input => Some(Causality::Input),
                        fmi3::schema::Causality::Output => Some(Causality::Output),
                        fmi3::schema::Causality::CalculatedParameter
                        | fmi3::schema::Causality::StructuralParameter
                        | fmi3::schema::Causality::Local
                        | fmi3::schema::Causality::Independent
                        | fmi3::schema::Causality::Dependent => None,
                    }?;
                    Some(Port { name: variable.name().to_owned(), causality, reference: variable.value_reference(), unit: variable.declared_type().and_then(|declared| units.get(declared).copied()) })
                })
                .collect();
            let instance = import.instantiate_cs(&description.model_name, false, false, false, false, &[]).map_err(failure(Operation::Instantiate(path.to_path_buf())))?;
            Ok(Fmu { instance: Instance::Fmi3 { instance: Box::new(instance), import }, ports })
        }
        major => Err(DynamicsError::Version { path: path.to_path_buf(), major }),
    }
}

const fn failure<E>(operation: Operation) -> impl FnOnce(E) -> DynamicsError
where E: Into<fmi::Error> {
    move |source| DynamicsError::Fmi { operation, source: source.into() }
}
