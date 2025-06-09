export type FormElement = {
    name: string;
    dataType?: string | null;
    orderIndex: number;
    children: FormElement[];
}

export type FICStructure = {
    systemId: string | null;
    name: string;
    status: number;
    formElements: FormElement[];
}
export type TemplateUpdateStatusModel ={
    id: string;
    status: number;
}

export const SampleFICStructure = {
    systemId: null,
    name: "My Form Structure",
    status: 1,
    formElements: [
        {
            name: "I. ",
            dataType: null,
            orderIndex: 0,
            children: [
                {
                    name: "1. ",
                    dataType: null,
                    orderIndex: 0,
                    children: [
                        {
                            name: "1.1. ",
                            dataType: 'boolean',
                            orderIndex: 0,
                            children: []
                        },
                        {
                            name: "1.2. .",
                            dataType: 'text',
                            orderIndex: 1,
                            children: []
                        }
                    ]
                }
            ]
        }
    ]
}