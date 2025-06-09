import { z } from 'zod';
import { HttpMethodType } from '../../../enum/enum';

export const featureSchema = z.object({
  featureName: z.string()
    .min(5, { message: "Feature name must be 5-50 characters long" })
    .max(50, { message: "Feature name must be 5-50 characters long" })
    .refine(value => value.trim().length > 0, {
      message: "Feature name cannot be only whitespace"
    }),
  parentId: z.union([z.string(), z.number(), z.null()]).optional().nullable(),
  url: z.string().max(200, { message: "URL must be 5-200 characters long" }).optional().nullable(),
  description: z.string().max(200, { message: "Description must be at most 200 characters long" }).optional().nullable(),
  httpMethod: z.nativeEnum(HttpMethodType).optional().nullable(),
}).refine((data) => {
  if (data.parentId !== null && data.parentId !== undefined) {
    return data.url && data.httpMethod !== null && data.httpMethod !== undefined;
  }
  return true;
}, {
  message: "URL and HTTP Method are required when Parent ID is set",
  path: ["url"], // You can also use ["httpMethod"] or leave it as [""] for a global error
});

export type FeaturePostModel = z.infer<typeof featureSchema>;
