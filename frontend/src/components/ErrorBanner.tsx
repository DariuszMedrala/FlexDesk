import { ApiError } from '../api/client'

export function ErrorBanner({ error }: { error: unknown }) {
  if (!error) {
    return null
  }

  let message = 'Something went wrong.'
  let details: string[] = []

  if (error instanceof ApiError) {
    message = error.message
    if (error.errors) {
      details = Object.values(error.errors).flat()
    }
  } else if (error instanceof Error) {
    message = error.message
  }

  return (
    <div className="banner banner-error" role="alert">
      <strong>{message}</strong>
      {details.length > 0 && (
        <ul>
          {details.map((detail) => (
            <li key={detail}>{detail}</li>
          ))}
        </ul>
      )}
    </div>
  )
}
