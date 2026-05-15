export type SampleWorkOrderDto = {
	id: number;
	workOrderNumber: string;
	customerName: string;
	status: string;
	createdOnUtc: string;
	dueDateUtc?: string | null;
	description?: string | null;
};
