import { useQuery } from '@tanstack/vue-query'
import { sessionMeGetOptions } from '~~/generated/api/@tanstack/vue-query.gen'

export const useSession = () => useQuery({ ...sessionMeGetOptions(), retry: false })
