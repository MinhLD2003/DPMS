import { FormElement } from "../pages/FIC/models/FICSubmissionViewModel";
export const sortFormElements = (elements: FormElement[]): FormElement[] => {
    return elements
        .sort((a, b) => a.orderIndex - b.orderIndex)
        .map((element) => ({
            ...element,
            children: element.children && element.children.length > 0 ? sortFormElements(element.children) : [],
        }));
};
export const updateFormElementValue = (
    elements: FormElement[],
    id: string,
    value: string | null
): FormElement[] => {
    return elements.map((el) => ({
        ...el,
        value: el.id === id ? value : el.value,
        children: el.children.length > 0 ? updateFormElementValue(el.children, id, value) : el.children,
    }));
};

export const extractDeepestChildren = (
    formElements: FormElement[]
): { formElementId: string; value: string | null }[] => {
    const responses: { formElementId: string; value: string | null }[] = [];

    const traverse = (elements: FormElement[]) => {
        for (const element of elements) {
            if (!element.children || element.children.length === 0) {
                responses.push({
                    formElementId: element.id,
                    value: element.value,
                });
            } else {
                traverse(element.children);
            }
        }
    };

    traverse(formElements);
    return responses;
};
