import { ApiError } from '../api/client'

export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.errors) {
      const details = Object.values(error.errors).flat()
      if (details.length > 0) {
        return details.join(' ')
      }
    }
    return error.message
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Something went wrong.'
}
