import { useFeedView } from './useFeedView'

export function useReadMode() {
  const { readMode } = useFeedView()
  return { readMode }
}
