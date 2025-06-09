export type FormElement = {
    id:string;
    name: string;
    dataType: string | null;
    value: string | null;
    orderIndex: number;
    children: FormElement[];
  }
  
export type FICStructure = {
    systemId: string;
    name: string;
    formElements: FormElement[];
  }
  