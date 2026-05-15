<script setup lang="ts">
	import { ref, computed, onMounted } from "vue";
	import type { SampleWorkOrderDto } from "../types/sampleWorkOrders";

	const props = defineProps<{
		listUrl: string;
		detailsBaseUrl: string; // like "/SampleWorkOrders/Details"
	}>();

	const loading = ref(true);
	const error = ref<string | null>(null);
	const query = ref("");
	const items = ref<SampleWorkOrderDto[]>([]);

	const filtered = computed(() => {
		const q = query.value.trim().toLowerCase();
		if (!q) return items.value;

		return items.value.filter(x =>
			(x.workOrderNumber ?? "").toLowerCase().includes(q) ||
			(x.customerName ?? "").toLowerCase().includes(q) ||
			(x.status ?? "").toLowerCase().includes(q)
		);
	});

	onMounted(async () => {
		try {
			const res = await fetch(props.listUrl, { headers: { Accept: "application/json" } });
			if (!res.ok) throw new Error(`HTTP ${res.status}`);
			items.value = await res.json();
		} catch (e) {
			error.value = e instanceof Error ? e.message : "Failed to load.";
		} finally {
			loading.value = false;
		}
	});
</script>


<template>
	<div class="space-y-3">
		<!-- Demo banner -->
		<div class="bg-red-500 text-white p-2">
			Vue island mounted
		</div>

		<div class="flex items-center justify-between gap-3">
			<input v-model="query"
				   class="app-input"
				   placeholder="Search by WO #, customer, status..." />
		</div>

		<div v-if="loading" class="text-sm app-text-muted">Loading...</div>
		<div v-else-if="error" class="text-sm text-red-600">{{ error }}</div>

		<div v-else class="overflow-x-auto">
			<table class="min-w-full text-sm">
				<thead>
					<tr class="text-left border-b" style="border-color: var(--color-border);">
						<th class="py-2 pr-3">Work Order</th>
						<th class="py-2 pr-3">Customer</th>
						<th class="py-2 pr-3">Status</th>
						<th class="py-2 pr-3">Created</th>
						<th class="py-2 pr-3">Due</th>
						<th class="py-2 pr-3"></th>
					</tr>
				</thead>

				<tbody>
					<tr v-for="x in filtered"
						:key="x.id"
						class="border-b"
						style="border-color: var(--color-border);">
						<td class="py-2 pr-3 font-medium">{{ x.workOrderNumber }}</td>
						<td class="py-2 pr-3">{{ x.customerName }}</td>
						<td class="py-2 pr-3">
							<span class="inline-flex rounded-full px-2 py-0.5 text-xs border"
								  style="border-color: var(--color-border);">
								{{ x.status }}
							</span>
						</td>
						<td class="py-2 pr-3">{{ new Date(x.createdOnUtc).toLocaleString() }}</td>
						<td class="py-2 pr-3">
							{{ x.dueDateUtc ? new Date(x.dueDateUtc).toLocaleString() : "-" }}
						</td>
						<td class="py-2 pr-3 text-right">
							<a class="text-brand hover:underline" :href="props.detailsBaseUrl + '/' + x.id">
								Details
							</a>
						</td>
					</tr>
				</tbody>
			</table>

			<div v-if="filtered.length === 0" class="text-sm app-text-muted pt-3">
				No results.
			</div>
		</div>
	</div>
</template>
