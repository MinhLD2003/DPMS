import { FormElement } from "../pages/FIC/models/FICTemplateViewModel";

export const sortFormElements = (elements: FormElement[]): FormElement[] => {
    return elements
        .sort((a, b) => a.orderIndex - b.orderIndex)
        .map((element) => {
            if (element.children && element.children.length > 0) {
                return {
                    ...element,
                    children: sortFormElements(element.children),
                };
            }
            return element;
        });
};

export const getTableData = (elements: FormElement[], parentKey: string = ""): any[] => {
    let data: any[] = [];

    elements.forEach((element, index) => {
        const key = `${parentKey}${index}`;

        if (element.dataType === 'Boolean') {
            data.push({
                key,
                name: element.name,
                input: element.children.length === 0 ? element.id : null,
                type: "Boolean",
                hasChildren: element.children.length > 0
            });
        }
        if (element.dataType === 'Text') {
            data.push({
                key,
                name: element.name,
                input: element.children.length === 0 ? element.id : null,
                type: "Text",
                hasChildren: element.children.length > 0
            });
        }
        if (element.dataType === null) {
            data.push({
                key,
                name: element.name,
                input: null,
                type: "Null",
                hasChildren: element.children.length > 0
            });
        }
        if (element.children.length > 0) {
            data = data.concat(getTableData(element.children, `${key}-`));
        }
    });

    return data;
};
