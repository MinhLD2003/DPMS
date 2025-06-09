import { useState, useEffect } from 'react';
import { Form, FormInstance } from 'antd';
import { z, ZodType } from 'zod';

// Type definitions
type ErrorRecord<T> = {
  [P in keyof T]?: string;
};

type FormattedErrorShape = {
  _errors: string[];
  [key: string]: any;
};

type ValidationOptions = {
  validateOnChange: boolean;
};

type ValidationHookReturn<T> = {
  errors: ErrorRecord<T>;
  validateForm: () => boolean;
  validateField: (fieldName: keyof T) => boolean;
};

/**
 * Custom hook for Zod form validation with improved UX
 * @param form - Ant Design form instance
 * @param schema - Zod schema for validation
 * @param options - Validation options
 * @returns Validation state and methods
 */
export function useFormValidation<T extends Record<string, any>>(
  form: FormInstance<T>,
  schema: ZodType<T>,
  options: ValidationOptions = { validateOnChange: false }
): ValidationHookReturn<T> {
  const values = Form.useWatch([], form) as T | undefined;
  const [errors, setErrors] = useState<ErrorRecord<T>>({});
  const [touchedFields, setTouchedFields] = useState<Set<keyof T>>(new Set());

  /**
   * Validates the entire form and updates error state
   * @returns Whether the form is valid
   */
  const validateForm = (): boolean => {
    if (!values) return false;

    const result = schema.safeParse(values);
    if (result.success) {
      setErrors({});
      return true;
    } else {
      const formattedErrors = result.error.format() as Record<string, FormattedErrorShape>;
      const newErrors: ErrorRecord<T> = {};

      Object.keys(formattedErrors).forEach((key) => {
        if (key !== '_errors') {
          const errorObj = formattedErrors[key];
          if (errorObj && errorObj._errors && errorObj._errors.length > 0) {
            newErrors[key as keyof T] = errorObj._errors[0];
          }
        }
      });

      setErrors(newErrors);
      return false;
    }
  };

  /**
   * Validates a single field and updates its error state
   * @param fieldName - The name of the field to validate
   * @returns Whether the field is valid
   */
  const validateField = (fieldName: keyof T): boolean => {
    if (!values) return false;

    // Mark the field as touched
    setTouchedFields(prev => {
      const updated = new Set(prev);
      updated.add(fieldName);
      return updated;
    });

    // Get the field schema from the main schema
    // Note: This is a simplification, as extracting a single field's schema
    // from a complex Zod schema can be more complicated
    const fieldSchema = z.object({ [fieldName as string]: schema.shape?.[fieldName as string] })
      .pick({ [fieldName as string]: true });

    // Validate just this field
    const result = fieldSchema.safeParse({ [fieldName]: values[fieldName as string] });

    if (result.success) {
      setErrors(prev => {
        const updated = { ...prev };
        delete updated[fieldName];
        return updated;
      });
      return true;
    } else {
      const formatted = result.error.format();
      const fieldErrors = formatted[fieldName as string] as FormattedErrorShape | undefined;
      const errorMessage = fieldErrors?._errors[0] || 'Invalid input';

      setErrors(prev => ({
        ...prev,
        [fieldName]: errorMessage
      }));
      return false;
    }
  };

  // Run validation on value changes if validateOnChange is true
  useEffect(() => {
    if (options.validateOnChange && values) {
      // Only validate touched fields when values change
      touchedFields.forEach(field => {
        validateField(field);
      });

      // Check overall form validity but don't update error messages for untouched fields
      const result = schema.safeParse(values);
    }
  }, [values, schema, touchedFields, options.validateOnChange]);

  return {errors, validateForm, validateField };
}