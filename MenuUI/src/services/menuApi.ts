import { components, paths } from "@/generated/open-api/menu-api"
import { Fetcher } from "@qdrant/openapi-typescript-fetch"

const fetcher = Fetcher.for<paths>()

fetcher.configure({
  baseUrl:"https://localhost:44347"
})

export const getRecipes = fetcher.path("/api/Recipe").method("get").create();
export type Recipe = components["schemas"]["Recipe"];
