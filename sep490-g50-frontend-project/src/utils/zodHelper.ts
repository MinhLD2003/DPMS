import { ZodError } from 'zod';

export const mapZodErrors = (error: ZodError) => {
  const formattedErrors: Record<string, string> = {};
  error.errors.forEach((e) => {
    formattedErrors[e.path[0]] = e.message;
  });
  return formattedErrors;
};
