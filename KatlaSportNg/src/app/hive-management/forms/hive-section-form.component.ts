import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HiveSectionService } from '../services/hive-section.service';
import { HiveSection } from '../models/hive-section';

@Component({
  selector: 'app-hive-section-form',
  templateUrl: './hive-section-form.component.html',
  styleUrls: ['./hive-section-form.component.css']
})

export class HiveSectionFormComponent implements OnInit {
  hiveId: number;
  section: HiveSection = new HiveSection();
  isNewSection: boolean;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private hiveSectionService: HiveSectionService
  ) { }

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.hiveId = +params['hiveId'];
      const sectionId = +params['sectionId'];
      if (sectionId) {
        this.isNewSection = false;
        this.hiveSectionService.getHiveSection(sectionId).subscribe(section => {
          if (section) {
            this.section = section;
          }
        });
      } else {
        this.isNewSection = true;
      }
    });
  }

  navigateToSections() {
    this.router.navigate(['/hive', this.hiveId, 'sections']);
  }

  onSubmit() {
    console.log(this.isNewSection)
    const operation = this.isNewSection ? this.hiveSectionService.addHiveSection(this.hiveId, this.section) : this.hiveSectionService.updateHiveSection(this.section.id, this.section);
    operation.subscribe(() => this.navigateToSections());
  }
  
  onDelete() {
    this.hiveSectionService.setHiveSectionStatus(this.section.id, true).subscribe(h => this.section.isDeleted = true);
  }
  
  onUndelete() {
    this.hiveSectionService.setHiveSectionStatus(this.section.id, false).subscribe(h => this.section.isDeleted = false);
  }
  
  onPurge() {
    this.hiveSectionService.deleteHiveSection(this.section.id).subscribe(() => { this.navigateToSections(); });
  }
}
