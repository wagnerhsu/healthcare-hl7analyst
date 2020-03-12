/***************************************************************
* Copyright (C) 2011 Jeremy Reagan, All Rights Reserved.
* I may be reached via email at: jeremy.reagan@live.com
* 
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; under version 2
* of the License.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
****************************************************************/

#region

using System.Collections.Generic;

#endregion

namespace HL7_Analyst
{
    /// <summary>
    /// MessageType Class: Used to store message type descriptions
    /// </summary>
    internal class MessageType
    {
        /// <summary>
        /// MessageType constructor
        /// </summary>
        /// <param name="msgType">The message type name</param>
        public MessageType(string msgType)
        {
            Type = msgType;
            Description = GetMessageDescription(msgType);
        }

        /// <summary>
        /// The message type name
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The message type description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Loads and pulls the specified MessageType description
        /// </summary>
        /// <param name="msgType">The message type to load</param>
        /// <returns>The message type description</returns>
        private string GetMessageDescription(string msgType)
        {
            var dic = new Dictionary<string, string>
            {
                {"A01", "Admit/visit notification"},
                {"A02", "Transfer a patient"},
                {"A03", "Discharge/end visit"},
                {"A04", "Register a patient"},
                {"A05", "Pre-admit a patient"},
                {"A06", "Change an outpatient to an inpatient"},
                {"A07", "Change an inpatient to an outpatient"},
                {"A08", "Update patient information"},
                {"A09", "Patient departing - tracking"},
                {"A10", "Patient arriving - tracking"},
                {"A11", "Cancel admit/visit notification"},
                {"A12", "Cancel transfer"},
                {"A13", "Cancel discharge/end visit"},
                {"A14", "Pending admit"},
                {"A15", "Pending transfer"},
                {"A16", "Pending discharge"},
                {"A17", "Swap patients"},
                {"A18", "Merge patient information (for backward compatibility only)"},
                {"A19", "QRY/ADR - Patient query"},
                {"A20", "Bed status update"},
                {"A21", "Patient goes on a leave of absence"},
                {"A22", "Patient returns from a leave of absence"},
                {"A23", "Delete a patient record"},
                {"A24", "Link patient information"},
                {"A25", "Cancel pending discharge"},
                {"A26", "Cancel pending transfer"},
                {"A27", "Cancel pending admit"},
                {"A28", "Add person information"},
                {"A29", "Delete person information"},
                {"A30", "Merge person information (for backward compatibility only)"},
                {"A31", "Update person information"},
                {"A32", "Cancel patient arriving - tracking"},
                {"A33", "Cancel patient departing - tracking"},
                {"A34", "Merge patient information - patient ID only (for backward compatibility only)"},
                {"A35", "Merge patient information - account number only (for backward compatibility only)"},
                {"A36", "Merge patient information - patient ID and account number (for backward compatibility only)"},
                {"A37", "Unlink patient information"},
                {"A38", "Cancel pre-admit"},
                {"A39", "Merge person - patient ID (for backward compatibility only)"},
                {"A40", "Merge patient - patient identifier list"},
                {"A41", "Merge account - patient account number"},
                {"A42", "Merge visit - visit number"},
                {"A43", "Move patient information - patient identifier list"},
                {"A44", "Move account information - patient account number"},
                {"A45", "Move visit information - visit number"},
                {"A46", "Change patient ID (for backward compatibility only)"},
                {"A47", "Change patient identifier list"},
                {"A48", "Change alternate patient ID (for backward compatibility only)"},
                {"A49", "Change patient account number"},
                {"A50", "Change visit number"},
                {"A51", "Change alternate visit ID"},
                {"A52", "Cancel leave of absence for a patient"},
                {"A53", "Cancel patient returns from a leave of absence"},
                {"A54", "Change attending doctor"},
                {"A55", "Cancel change attending doctor"},
                {"A60", "Update allergy information"},
                {"A61", "Change consulting doctor"},
                {"A62", "Cancel change consulting doctor"},
                {"B01", "Add personnel record"},
                {"B02", "Update personnel record"},
                {"B03", "Delete personnel re cord"},
                {"B04", "Active practicing person"},
                {"B05", "Deactivate practicing person"},
                {"B06", "Terminate practicing person"},
                {"B07", "Grant Certificate/Permission"},
                {"B08", "Revoke Certificate/Permission"},
                {"C01", "Register a patient on a clinical trial"},
                {"C02", "Cancel a patient registration on clinical trial (for clerical mistakes only)"},
                {"C03", "Correct/update registration information"},
                {"C04", "Patient has gone off a clinical trial"},
                {"C05", "Patient enters phase of clinical trial"},
                {"C06", "Cancel patient entering a phase (clerical mistake)"},
                {"C07", "Correct/update phase information"},
                {"C08", "Patient has gone off phase of clinical trial"},
                {"C09", "Automated time intervals for reporting, like monthly"},
                {"C10", "Patient completes the clinical trial"},
                {"C11", "Patient completes a phase of the clinical trial"},
                {"C12", "Update/correction of patient order/result information"},
                {"E01", "Submit HealthCare Services Invoice"},
                {"E02", "Cancel HealthCare Services Invoice"},
                {"E03", "HealthCare Services Invoice Status"},
                {"E04", "Re-Assess HealthCare Services Invoice Request"},
                {"E10", "Edit/Adjudication Results"},
                {"E12", "Request Additional Information"},
                {"E13", "Additional Information Response"},
                {"E15", "Payment/Remittance Advice"},
                {"E20", "Submit Authorization Request"},
                {"E21", "Cancel Authorization Request"},
                {"E22", "Authorization Request Status"},
                {"E24", "Authorization Response"},
                {"E30", "Submit Health Document related to Authorization Request"},
                {"E31", "Cancel Health Document related to Authorization Request"},
                {"I01", "Request for insurance information"},
                {"I02", "Request/receipt of patient selection display list"},
                {"I03", "Request/receipt of patient selection list"},
                {"I04", "Request for patient demographic data"},
                {"I05", "Request for patient clinical information"},
                {"I06", "Request/receipt of clinical data listing"},
                {"I07", "PIN/ACK - Unsolicited insurance information"},
                {"I08", "Request for treatment authorization information"},
                {"I09", "Request for modification to an authorization"},
                {"I10", "Request for resubmission of an authorization"},
                {"I11", "Request for cancellation of an authorization"},
                {"I12", "Patient referral"},
                {"I13", "Modify patient referral"},
                {"I14", "Cancel patient referral"},
                {"I15", "Request patient referral status"},
                {"J01", "Cancel query/acknowledge message"},
                {"J02", "Cancel subscription/acknowledge message"},
                {"K11", "Segment pattern response in response to QBP^Q11"},
                {"K13", "Tabular response in response to QBP^Q13"},
                {"K15", "Display response in response to QBP^Q15"},
                {"K21", "Get person demographics response"},
                {"K22", "Find candidates response"},
                {"K23", "Get corresponding identifiers response"},
                {"K24", "Allocate identifiers response"},
                {"K25", "Personnel Information by Segment Response"},
                {"K31", "Dispense History Response"},
                {"M01", "Master file not otherwise specified (for backward compatibility only)"},
                {"M02", "Master file - staff practitioner"},
                {"M03", "Master file - test/observation (for backward compatibility only)"},
                {"M04", "Master files charge description"},
                {"M05", "Patient location master file"},
                {"M06", "Clinical study with phases and schedules master file"},
                {"M07", "Clinical study without phases but with schedules master file"},
                {"M08", "Test/observation (numeric) master file"},
                {"M09", "Test/observation (categorical) master file"},
                {"M10", "Test /observation batteries master file"},
                {"M11", "Test/calculated observations master file"},
                {"M12", "Master file notification message"},
                {"M13", "Master file notification - general"},
                {"M14", "Master file notification - site defined"},
                {"M15", "Inventory item master file notification"},
                {"M16", "Master File Notification Inventory Item Enhanced"},
                {"M17", "DRG Master File Message"},
                {"N01", "Application management query message"},
                {"N02", "Application management data message (unsolicited)"},
                {"O01", "Order message (also RDE, RDS, RGV, RAS)"},
                {"O02", "Order response (also RRE, RRD, RRG, RRA)"},
                {"O03", "Diet order"},
                {"O04", "Diet order acknowledgment"},
                {"O05", "Stock requisition order"},
                {"O06", "Stock requisition acknowledgment"},
                {"O07", "Non-stock requisition order"},
                {"O08", "Non-stock requisition acknowledgment"},
                {"O09", "Pharmacy/treatment order"},
                {"O10", "Pharmacy/treatment order acknowledgment"},
                {"O11", "Pharmacy/treatment encoded order"},
                {"O12", "Pharmacy/treatment encoded order acknowledgment"},
                {"O13", "Pharmacy/treatment dispense"},
                {"O14", "Pharmacy/treatment dispense acknowledgment"},
                {"O15", "Pharmacy/treatment give"},
                {"O16", "Pharmacy/treatment give acknowledgment"},
                {"O17", "Pharmacy/treatment administration"},
                {"O18", "Pharmacy/treatment administration acknowledgment"},
                {"O19", "General clinical order"},
                {"O20", "General clinical order response"},
                {"O21", "Laboratory order"},
                {"O22", "General laboratory order response message to any OML"},
                {"O23", "Imaging order"},
                {"O24", "Imaging order response message to any OMI"},
                {"O25", "Pharmacy/treatment refill authorization request"},
                {"O26", "Pharmacy/Treatment Refill Authorization Acknowledgement"},
                {"O27", "Blood product order"},
                {"O28", "Blood product order acknowledgment"},
                {"O29", "Blood product dispense status"},
                {"O30", "Blood product dispense status acknowledgment"},
                {"O31", "Blood product transfusion/disposition"},
                {"O32", "Blood product transfusion/disposition acknowledgment"},
                {"O33", "Laboratory order for multiple orders related to a single specimen"},
                {"O34", "Laboratory order response message to a multiple order related to single specimen OML"},
                {"O35", "Laboratory order for multiple orders related to a single container of a specimen"},
                {"O36", "Laboratory order response message to a single container of a specimen OML"},
                {"O37", "Population/Location-Based Laboratory Order Message"},
                {"O38", "Population/Location-Based Laboratory Order Acknowledgment Message"},
                {"P01", "Add patient accounts"},
                {"P02", "Purge patient accounts"},
                {"P03", "Post detail financial transaction"},
                {"P04", "Generate bill and A/R statements"},
                {"P05", "Update account"},
                {"P06", "End account"},
                {"P07", "Unsolicited initial individual product experience report"},
                {"P08", "Unsolicited update individual product experience report"},
                {"P09", "Summary product experience report"},
                {"P10", "Transmit Ambulatory Payment Classification(APC)"},
                {"P11", "Post Detail Financial Transactions - New"},
                {"P12", "Update Diagnosis/Procedure"},
                {"PC1", "PC/ problem add"},
                {"PC2", "PC/ problem update"},
                {"PC3", "PC/ problem delete"},
                {"PC4", "PC/ problem query"},
                {"PC5", "PC/ problem response"},
                {"PC6", "PC/ goal add"},
                {"PC7", "PC/ goal update"},
                {"PC8", "PC/ goal delete"},
                {"PC9", "PC/ goal query"},
                {"PCA", "PC/ goal response"},
                {"PCB", "PC/ pathway (problem-oriented) add"},
                {"PCC", "PC/ pathway (problem-oriented) update"},
                {"PCD", "PC/ pathway (problem-oriented) delete"},
                {"PCE", "PC/ pathway (problem-oriented) query"},
                {"PCF", "PC/ pathway (problem-oriented) query response"},
                {"PCG", "PC/ pathway (goal-oriented) add"},
                {"PCH", "PC/ pathway (goal-oriented) update"},
                {"PCJ", "PC/ pathway (goal-oriented) delete"},
                {"PCK", "PC/ pathway (goal-oriented) query"},
                {"PCL", "PC/ pathway (goal-oriented) query response"},
                {"Q01", "Query sent for immediate response"},
                {"Q02", "Query sent for deferred response"},
                {"Q03", "Deferred response to a query"},
                {"Q05", "Unsolicited display update message"},
                {"Q06", "Query for order status"},
                {"Q11", "Query by parameter requesting an RSP segment pattern response"},
                {"Q13", "Query by parameter requesting an RTB - tabular response"},
                {"Q15", "Query by parameter requesting an RDY display response"},
                {"Q16", "Create subscription"},
                {"Q17", "Query for previous events"},
                {"Q21", "Get person demographics"},
                {"Q22", "Find candidates"},
                {"Q23", "Get corresponding identifiers"},
                {"Q24", "Allocate identifiers"},
                {"Q25", "Personnel Information by Segment Query"},
                {"Q26", "Pharmacy/treatment order response"},
                {"Q27", "Pharmacy/treatment administration information"},
                {"Q28", "Pharmacy/treatment dispense information"},
                {"Q29", "Pharmacy/treatment encoded order information"},
                {"Q30", "Pharmacy/treatment dose information"},
                {"Q31", "Query Dispense history"},
                {"R01", "Unsolicited transmission of an observation message"},
                {"R02", "Query for results of observation"},
                {"R04", "Response to query; transmission of requested observation"},
                {"R21", "Unsolicited laboratory observation"},
                {"R22", "Unsolicited Specimen Oriented Observation Message"},
                {"R23", "Unsolicited Specimen Container Oriented Observation Message"},
                {"R24", "Unsolicited Order Oriented Observation Message"},
                {"R25", "Unsolicited Population/Location-Based Laboratory Observation Message"},
                {"R30", "Unsolicited Point-Of-Care Observation Message Without Existing Order - Place An Order"},
                {"R31", "Unsolicited New Point-Of-Care Observation Message - Search For An Order"},
                {"R32", "Unsolicited Pre-Ordered Point-Of-Care Observation"},
                {"ROR", "Pharmacy prescription order query response"},
                {"S01", "Request new appointment booking"},
                {"S02", "Request appointment rescheduling"},
                {"S03", "Request appointment modification"},
                {"S04", "Request appointment cancellation"},
                {"S05", "Request appointment discontinuation"},
                {"S06", "Request appointment deletion"},
                {"S07", "Request addition of service/resource on appointment"},
                {"S08", "Request modification of service/resource on appointment"},
                {"S09", "Request cancellation of service/resource on appointment"},
                {"S10", "Request discontinuation of service/resource on appointment"},
                {"S11", "Request deletion of service/resource on appointment"},
                {"S12", "Notification of new appointment booking"},
                {"S13", "Notification of appointment rescheduling"},
                {"S14", "Notification of appointment modification"},
                {"S15", "Notification of appointment cancellation"},
                {"S16", "Notification of appointment discontinuation"},
                {"S17", "Notification of appointment deletion"},
                {"S18", "Notification of addition of service/resource on appointment"},
                {"S19", "Notification of modification of service/resource on appointment"},
                {"S20", "Notification of cancellation of service/resource on appointment"},
                {"S21", "Notification of discontinuation of service/resource on appointment"},
                {"S22", "Notification of deletion of service/resource on appointment"},
                {"S23", "Notification of blocked schedule time slot(s)"},
                {"S24", "Notification of opened (unblocked) schedule time slot(s)"},
                {"S25", "Schedule query message and response"},
                {"S26", "Notification that patient did not show up for schedule appointment"},
                {"S28", "Request new sterilization lot"},
                {"S29", "Request Sterilization lot deletion"},
                {"S30", "Request item"},
                {"S31", "Request anti-microbial device data"},
                {"S32", "Request anti-microbial device cycle data"},
                {"S33", "Notification of sterilization configuration"},
                {"S34", "Notification of sterilization lot"},
                {"S35", "Notification of sterilization lot deletion"},
                {"S36", "Notification of anti-microbial device data"},
                {"S37", "Notification of anti-microbial device cycle data"},
                {"T01", "Original document notification"},
                {"T02", "Original document notification and content"},
                {"T03", "Document status change notification"},
                {"T04", "Document status change notification and content"},
                {"T05", "Document addendum notification"},
                {"T06", "Document addendum notification and content"},
                {"T07", "Document edit notification"},
                {"T08", "Document edit notification and content"},
                {"T09", "Document replacement notification"},
                {"T10", "Document replacement notification and content"},
                {"T11", "Document cancel notification"},
                {"T12", "Document query"},
                {"U01", "Automated equipment status update"},
                {"U02", "Automated equipment status request"},
                {"U03", "Specimen status update"},
                {"U04", "specimen status request"},
                {"U05", "Automated equipment inventory update"},
                {"U06", "Automated equipment inventory request"},
                {"U07", "Automated equipment command"},
                {"U08", "Automated equipment response"},
                {"U09", "Automated equipment notification"},
                {"U10", "Automated equipment test code settings update"},
                {"U11", "Automated equipment test code settings request"},
                {"U12", "Automated equipment log/service update"},
                {"U13", "Automated equipment log/service request"},
                {"V01", "Query for vaccination record"},
                {"V02", "Response to vaccination query returning multiple PID matches"},
                {"V03", "Vaccination record response"},
                {"V04", "Unsolicited vaccination record update"},
                {"W01", "Waveform result, unsolicited transmission of requested information"},
                {"W02", "Waveform result, response to query"}
            };

            var returnValue = "";
            return dic.TryGetValue(msgType, out returnValue) ? returnValue : "";
        }
    }
}