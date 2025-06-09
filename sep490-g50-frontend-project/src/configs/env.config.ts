
import { ZodError } from "zod";
import { EnvConfig, EnvSchema } from "../validation/env.validation";

// Load environment variables
export const validateEnv = () => {
    try {
        // Parse and validate environment variables
        const envVars: EnvConfig = EnvSchema.parse(import.meta.env);
        //console.log("✅ Environment variables loaded and validated:", envVars);

        return {
            apiBaseUrl: envVars.VITE_PUBLIC_API_URL,
            googleClientId: envVars.VITE_GOOGLE_CLIENT_ID,
            geminiApiKey: envVars.VITE_GEMINI_API_KEY,
        };
    } catch (error) {
        if (error instanceof ZodError) {
            console.error("❌ Validation failed. Missing or invalid environment variables:");
            error.errors.forEach((err) => {
                console.error(`- ${err.path.join(".")}: ${err.message}`);
            });
        } else {
            console.error("❌ Unexpected error parsing environment variables:", error);
        }

        throw new Error("Environment validation failed. Check the console for details.");
    }
};

export const env = validateEnv();
