export class HiveSection {
    constructor(
        public id: number,
        public name: string,
        public code: string,
        public createdBy: number,
        public created: Date,
        public lastUpdatedBy: number,
        public lastUpdated: Date,
        public storeHiveId: number,
        public isDeleted: boolean
    ) { }
}
