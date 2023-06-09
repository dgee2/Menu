import { Middleware } from "@qdrant/openapi-typescript-fetch"

export const logRequestAndResponse: Middleware = async (url, init, next) => {
    console.log(`fetching ${url}`, { init })
    const response = await next(url, init)
    console.log(`fetched ${url}`, { response, init })
    return response
}
