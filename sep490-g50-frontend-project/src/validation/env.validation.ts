import { z } from "zod";

export const EnvSchema = z.object({
    VITE_PUBLIC_API_URL: z
        .string({ required_error: "PUBLIC_API_URL is required" })
        .url({ message: "PUBLIC_API_URL must be a valid URL" }),

    // Google Client ID
    VITE_GOOGLE_CLIENT_ID: z
        .string({ required_error: "GOOGLE_CLIENT_ID is required" }),
    VITE_GEMINI_API_KEY: z
        .string({ required_error: "GEMINI_API_KEY is required" }),
});

export type EnvConfig = z.infer<typeof EnvSchema>;