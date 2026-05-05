import annotationlib


def read_annotations(obj: object) -> dict[str, object]:
    return annotationlib.get_annotations(obj)
